# SileroVAD-For-Unity

This is a project that reconstructs the C# code of Silero VAD into Unity.
This project is based on the [Silero VAD (C#)](https://github.com/snakers4/silero-vad/tree/master/examples/csharp) project, ported to work in real-time within the Unity engine.

---

## 📌 Features

- Real-time microphone input Voice Activity Detection (VAD)
- Supports `float[]` PCM audio buffer
- Integrates with Unity `AudioClip`, `Microphone`, etc.
- Lightweight and optimized for live applications

---

## 🔧 Requirements

- Unity 2021.3 LTS or later
- .NET Standard 2.0 compatible C# runtime

---

## 🚀 Getting Started

1. Clone this repository:

   ```bash
   git clone https://github.com/MelissaJSM/SileroVAD-For-Unity.git
   ```

2. Open the project in Unity.

3. Start the SileroVAD Scene.

4. The audio file supports a sampling rate of 16000 Hz and a monochannel .wav File.

---

## 📂 Project Structure

```
/Assets
  /Scripts
    SileroVadDetector.cs
    SileroVadRunner.cs        
    SileroSpeechSegment.cs
    SileroVadOnnxModel.cs
  /StreamingAssets
    silero_vad.onnx
    audioFile.wav
```

---

## 🔄 Based on / Acknowledgements

This project is based on the following:

- [Silero VAD (Python)](https://github.com/snakers4/silero-vad/tree/master/examples/csharp)  
  ⮑ Author: Silero Team  
  ⮑ License: MIT License

---

## 📄 License

This repository is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for details.

The original Silero VAD project is also under the MIT License.  
Please follow their repository’s terms when using model files and logic.

---

## ✨ Credits

- Original VAD logic and model: [Silero](https://github.com/snakers4/silero-vad)
- Unity C# Implementation: [MelissaJ](https://github.com/MelissaJSM)
