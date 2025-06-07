# Driver Assist Development

## Command Prompt

Modify `.vscode/settings.json` with the path to the Derail Valley install.

## Run Tests

```
dotnet test
```

# Dev Testing

Run `build.bat` to build and install the mod. This builds the solution with `dotnet build` and copies the DLL to the `Mods` folder.

## Create release

Update `repository.json`.

```
{
  "Releases": [
    {
      "Id": "DriverAssist",
      "Version": "0.10.0",
      "DownloadUrl": "https://github.com/dtandersen/dv-driver-assist/releases/download/v0.10.0/DriverAssist-v0.10.0.zip"
    },
}
```

Run `release.bat`.

Create a `v#.#.#` git tag and push the tag.

Upload zip to github.
