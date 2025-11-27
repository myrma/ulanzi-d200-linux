"""Background daemon for Ulanzi Manager"""

import sys
import time
import logging
import signal
from pathlib import Path
from typing import Optional

from ulanzi_manager.device import UlanziDevice, ButtonPress
from ulanzi_manager.config import ConfigParser, Config
from ulanzi_manager.actions import ActionExecutor

# Setup logging
log_dir = Path.home() / '.local/share/ulanzi'
log_dir.mkdir(parents=True, exist_ok=True)

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(log_dir / 'daemon.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)


class UlanziDaemon:
    """Background daemon for Ulanzi device"""

    def __init__(self, config_path: str):
        """Initialize daemon"""
        self.config_path = config_path
        self.config: Optional[Config] = None
        self.device: Optional[UlanziDevice] = None
        self.executor: Optional[ActionExecutor] = None
        self.running = False
        self.obs_client = None

    def start(self):
        """Start the daemon"""
        logger.info("Starting Ulanzi daemon...")

        try:
            # Load configuration
            self.config = ConfigParser.load(self.config_path)

            # Validate configuration
            errors = ConfigParser.validate(self.config)
            if errors:
                logger.error("Configuration errors:")
                for error in errors:
                    logger.error(f"  - {error}")
                return False

            # Connect to device
            self.device = UlanziDevice()
            self.device.set_button_callback(self._on_button_press)

            # Initialize OBS client if configured
            self._init_obs_client()

            # Initialize action executor
            self.executor = ActionExecutor(self.obs_client)

            # Configure device
            self._configure_device()

            self.running = True
            logger.info("Daemon started successfully")
            return True

        except Exception as e:
            logger.error(f"Failed to start daemon: {e}")
            return False

    def stop(self):
        """Stop the daemon"""
        logger.info("Stopping daemon...")
        self.running = False

        if self.device:
            self.device.close()

        if self.obs_client:
            try:
                self.obs_client.disconnect()
            except:
                pass

        logger.info("Daemon stopped")

    def run(self):
        """Run the daemon main loop"""
        if not self.start():
            return

        # Setup signal handlers
        signal.signal(signal.SIGTERM, lambda s, f: self.stop())
        signal.signal(signal.SIGINT, lambda s, f: self.stop())

        try:
            while self.running:
                # Read button presses (non-blocking)
                self.device.read_button_press()

                # Keep-alive
                self.device.set_small_window_data({})

                time.sleep(0.1)

        except KeyboardInterrupt:
            logger.info("Interrupted by user")
        except Exception as e:
            logger.error(f"Daemon error: {e}")
        finally:
            self.stop()

    def _init_obs_client(self):
        """Initialize OBS WebSocket client"""
        try:
            import obsws_python as obs

            self.obs_client = obs.ReqClient(
                host=self.config.obs_host,
                port=self.config.obs_port,
                password=self.config.obs_password,
                timeout=3
            )
            logger.info(f"Connected to OBS at {self.config.obs_host}:{self.config.obs_port}")
        except ImportError:
            logger.warning("obsws-python not installed, OBS features disabled")
        except ConnectionRefusedError:
            logger.warning(f"Could not connect to OBS at {self.config.obs_host}:{self.config.obs_port} - is it running?")
        except Exception as e:
            logger.warning(f"Failed to connect to OBS: {type(e).__name__}: {e}")

    def _configure_device(self):
        """Configure device with settings from config"""
        try:
            # Set brightness
            self.device.set_brightness(self.config.brightness)

            # Set label style
            if self.config.label_style:
                self.device.set_label_style(self.config.label_style)

            # Set button images
            button_dict = {}
            for button in self.config.buttons:
                button_dict[button.index] = {
                    'image': button.image,
                    'label': button.label,
                    'state': button.state
                }

            if button_dict:
                self.device.set_buttons(button_dict)

            logger.info("Device configured successfully")
        except Exception as e:
            logger.error(f"Failed to configure device: {e}")

    def _on_button_press(self, button: ButtonPress):
        """Handle button press event"""
        logger.info(f"Button {button.index} pressed (state={button.state})")

        # Find button config
        button_config = None
        for btn in self.config.buttons:
            if btn.index == button.index:
                button_config = btn
                break

        if not button_config:
            logger.warning(f"No config for button {button.index}")
            return

        logger.info(f"Executing action: {button_config.action_type} - {button_config.label}")

        # Execute action
        if self.executor and not button.pressed:
            self.executor.execute(button_config.action_type, button_config.action_params)


def main():
    """Main entry point"""
    import argparse

    parser = argparse.ArgumentParser(description='Ulanzi D200 daemon')
    parser.add_argument('config', help='Path to configuration file')
    parser.add_argument('--log-level', default='INFO', help='Logging level')
    args = parser.parse_args()

    # Set log level
    logging.getLogger().setLevel(getattr(logging, args.log_level.upper()))

    # Create and run daemon
    daemon = UlanziDaemon(args.config)
    daemon.run()


if __name__ == '__main__':
    main()
