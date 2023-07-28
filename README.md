# Digital Voice Modem FNE Audio Bridge

The Digital Voice Modem FNE Audio Bridge is a helper application designed to allow Rx and Tx of digital audio from/to PCM audio devices or UDP audio streams.

**NOTE**: This project relies on C++/CLI for interop with the vocoder library, as such it will not function or compile on a Linux/Unix system and requires Windows to function.

## Command Line Parameters

```
usage: dvmbridge [-h | --help][-c | --config <path to configuration file>][-l | --log-on-console][-i <wave in device no.>][-o <wave out device no.>]

Options:
  -h, --help                 show this message and exit
  -c, --config=VALUE         sets the path to the configuration file
  -l, --log-on-console       shows log on console
  -i, --input-device=VALUE   audio input device
  -o, --output-device=VALUE  audio output device

Audio Input Devices:
    ...

Audio Output Devices:
    ...
```

## License

This project is licensed under the GPLv3 and AGPLv3 License - see the [LICENSE.md](LICENSE.md) file for details. Use of this project is intended, strictly for amateur and educational use ONLY. Any other use is at the risk of user and all commercial purposes are strictly forbidden.

