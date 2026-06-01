# Vimera - Advanced Hash Analysis Software

[![GitHub downloads](https://img.shields.io/github/downloads/turkaysoft/vimera/total?style=flat&color=1a893c&label=Downloads)](https://github.com/turkaysoft/vimera/releases)
[![GitHub stars](https://img.shields.io/github/stars/turkaysoft/vimera?style=flat&color=0062cc&label=Stars)](https://github.com/turkaysoft/vimera/stargazers)
[![GitHub release](https://img.shields.io/github/v/release/turkaysoft/vimera?style=flat&color=5a32a3&label=Latest%20Release)](https://github.com/turkaysoft/vimera/releases/latest)
[![Platform](https://img.shields.io/badge/platform-Windows-b31d28?style=flat&label=Platform)](https://github.com/turkaysoft/vimera)

**Vimera** is a professional-grade **hash analysis software** developed by **Eray Türkay**. Designed for data integrity and security verification, Vimera allows you to generate, analyze, and compare file and text hashes with extreme speed. Whether you are verifying a large download or comparing sensitive data, Vimera provides the precision you need.

---

### Donate
You can support this project by making a donation to help ensure its sustainability and the development of new features.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-Donate-0a6628?style=flat&logo=buy-me-a-coffee&logoColor=white)](https://buymeacoffee.com/turkaysoft)

---

## Key Features

* **Integrity Focused:** Verify file and data authenticity with high-precision hash algorithms.
* **Pure Performance:** Developed exclusively in **C# and .NET Framework** with **zero external libraries** for maximum efficiency.
* **Multi-Hash Algorithm Support:** Generate and verify **CRC32, CRC64, MD5, SHA-1, SHA-256, SHA-384, SHA-512** hashes for both files and text with a single click.
* **Large File Optimization:** Uses streaming buffered reading to process files of **any size** (GB+) without loading entire content into memory — preventing out-of-memory crashes.
* **Binary Comparison Engine:** Compare two files or two hash strings at the byte level to verify identical content, with instant visual feedback on match/failure.
* **Drag & Drop Support:** Simply drag any file into the application window to instantly calculate its hash values — no manual path selection needed.
* **Parallel Hash Generation:** Calculates all selected hash types simultaneously using parallel processing, delivering results significantly faster than sequential generation.
* **Clipboard Integration:** Copy any generated hash to the clipboard with one click, or paste a hash string directly into the comparison tool for quick verification.
* **Real-Time Text Hashing:** As you type or modify text in the input field, hash values are updated instantly — providing immediate feedback without requiring a button click.
* **Export Results:** You can export the generated hash analysis results as the selected hash algorithm extension or as a text document for audit trails and documentation purposes.
* **Progress Tracking:** Displays visual progress and elapsed time for processing large files.
* **Modern UI:** Advanced interface featuring Light, Dark, and System theme support.
* **Multilingual:** It supports 15 different languages, primarily English. You can access the supported languages here: [Supported Languages](https://github.com/turkaysoft/vimera/discussions/2)
* **Portable:** No installation required. Extract the ZIP and start analyzing immediately.
* **Built-in Update Mechanism:** It features a built-in smart update mechanism developed specifically by **Türkaysoft**.

---

## Interface Preview

<img width="1010" height="633" alt="Vimera UI" src="https://github.com/user-attachments/assets/0fcacc0d-379d-49fb-91ef-3ce7666ec1fe" />

---

## Translation Support

* **Translation Support:** Community-driven localization via the official [Translation Guide](https://github.com/turkaysoft/vimera/discussions/2).

---

## System Requirements

| Feature | Minimum Requirements | Recommended Requirements |
| :--- | :--- | :--- |
| **OS** | Windows 10 20H2 x64 | Windows 10 22H2 x64 |
| **CPU** | x64 or ARM64 | x64 or ARM64 |
| **RAM** | 50 MB Free RAM | 75 MB Free RAM |
| **.NET** | .NET Framework 4.8.1 | .NET Framework 4.8.1 |

---

## Getting Started

1.  Navigate to the **[Releases](https://github.com/turkaysoft/vimera/releases/latest)** page.
2.  Download the latest ZIP file.
3.  **Extract all files from the ZIP** (Important: Application requires all folder contents to run correctly).
4.  Launch the executable corresponding to your architecture:
    * `Vimera_x64.exe`: For standard 64-bit Intel/AMD systems.
    * `Vimera_arm64.exe`: For ARM-based devices like Surface Pro.

---

## Security

* **Zero Data Export Policy:** Your privacy is our priority; no data leaves your machine.
* **No Dependencies:** Developed entirely from scratch using its own source code, there are no risks from security vulnerabilities in third-party libraries.
* **Open Source:** All source code for the program is open and can be reviewed by anyone.

---

## License

This software is offered free of charge as part of the **Türkaysoft solutions package** and is protected under the [**MIT License**](https://github.com/turkaysoft/vimera?tab=MIT-1-ov-file).
