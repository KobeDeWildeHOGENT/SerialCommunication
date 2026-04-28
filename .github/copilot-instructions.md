# Copilot instructions for SerialCommunication

Purpose
-------
Short, practical guidance so future Copilot sessions quickly understand this repo's structure, how to build/run pieces, and important conventions.

Quick build / run
-----------------
- Windows GUI (Visual Studio / MSBuild):
  - Build solution: msbuild SerialCommunication.slnx /p:Configuration=Debug
  - Build project only: msbuild SerialCommunication\SerialCommunication.csproj /p:Configuration=Debug
  - Run executable: SerialCommunication\bin\Debug\SerialCommunication.exe (or use VS Debug)
  - Target: .NET Framework 4.7.2 (ToolsVersion 15.0)

- Arduino sketch (SerialCommunication.ino):
  - Compile via Arduino IDE or arduino-cli. Example with arduino-cli:
    - Compile: arduino-cli compile --fqbn arduino:avr:uno C:\path\to\repo\SerialCommunication.ino
    - Upload: arduino-cli upload -p COM3 --fqbn arduino:avr:uno C:\path\to\repo\SerialCommunication.ino
  - Serial settings used in sketch: 115200 baud.

Tests & Lint
------------
- No automated tests or linters are configured in this repository. There is no test runner command to call.

High-level architecture
-----------------------
- Two primary components in the repo:
  1. Arduino firmware (root .ino + supporting C/C++ files)
     - SerialCommand library (SerialCommand.h / SerialCommand.cpp) is embedded and used to parse commands over the serial port.
     - analog.c contains an analogReadDelay helper (Arduino wiring-derived implementation).
     - Sketch registers command handlers: set, toggle, get, ping, help, debug. Baudrate 115200 and specific pin layout are hard-coded.
  2. Windows Forms app (SerialCommunication/)
     - A simple WinForms GUI that enumerates serial ports and sets baudrate (defaults to 115200).
     - Project targets .NET Framework 4.7.2 and is built with MSBuild/Visual Studio.

Key conventions and repo-specific patterns
----------------------------------------
- SerialCommand usage:
  - The sketch constructs a SerialCommand with the stream: SerialCommand sCmd(Serial);
  - Command handlers are registered via sCmd.addCommand("name", handler);
  - Default handler via sCmd.setDefaultHandler(...).
  - Command parsing uses whitespace delimiting and a terminator; handlers call sCmd.next() to read args.
- Strings in Arduino code use the F("...") macro to keep flash RAM usage low.
- Pin ranges used by the sketch:
  - Digital outputs: d2..d4
  - Digital inputs: d5..d7 (reads) and d2..d7 (get)
  - PWM outputs: pwm9..pwm11
  - Analog inputs: a0..a5
- Baudrate and pin assumptions are hard-coded; care when connecting hardware.

Other AI assistant configs
-------------------------
- No CLAUDE.md, AGENTS.md, .cursorrules, .windsurfrules, AIDER_CONVENTIONS.md, or similar assistant-specific config files were found.

If you want changes
-------------------
This file was added at .github/copilot-instructions.md. Ask if you want additional coverage (testing, CI, or more detailed command examples) and I will update it.
