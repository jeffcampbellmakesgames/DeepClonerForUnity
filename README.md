<a href="https://openupm.com/packages/com.jeffcampbellmakesgames.deepclonerforunity/"><img src="https://img.shields.io/npm/v/com.jeffcampbellmakesgames.deepclonerforunity?label=openupm&amp;registry_uri=https://package.openupm.com" /></a>
<img alt="GitHub issues" src="https://img.shields.io/github/issues/jeffcampbellmakesgames/DeepClonerForUnity">
<img alt="GitHub" src="https://img.shields.io/github/license/jeffcampbellmakesgames/DeepClonerForUnity">

# DeepCloner For Unity

## About
**DeepCloner For Unity** is a support library for easy shallow and deep copying of C# .Net objects in Unity. It is a fork of the original **DeepCloner** library that has additional support for ensuring `UnityEngine` type or derived type members on an object are assigned rather than shallow or deep-copied as many of these cannot be instantiated through plain C# construction.

## Installing DeepClonerForUnity
Using this library in your project can be done in three ways:

### Install via OpenUPM
The package is available on the [openupm registry](https://openupm.com/). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.jeffcampbellmakesgames.deepclonerforunity
```

### Install via GIT URL
Using the native Unity Package Manager introduced in 2017.2, you can add this library as a package by modifying your `manifest.json` file found at `/ProjectName/Packages/manifest.json` to include it as a dependency. See the example below on how to reference it.

```
{
	"dependencies": {
		...
		"com.jeffcampbellmakesgames.deepclonerforunity" : "https://github.com/jeffcampbellmakesgames/DeepClonerForUnity.git#release/stable",
		...
	}
}
```


You will need to have Git installed and available in your system's PATH.

### Install via classic `.UnityPackage`
The latest release can be found [here](https://github.com/jeffcampbellmakesgames/DeepClonerForUnity/releases) as a UnityPackage file that can be downloaded and imported directly into your project's Assets folder.

## Usage

To learn more about how to use JCMG DeepClonerForUnity, see [here](./usage.md) for more information.

## Support
If this is useful to you and/or you’d like to see future development and more tools in the future, please consider supporting it either by contributing to the Github projects (submitting bug reports or features and/or creating pull requests) or by buying me coffee using any of the links below. Every little bit helps!

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I3I2W7GX)

## Contributing

For information on how to contribute and code style guidelines, please visit [here](./contributing.md).