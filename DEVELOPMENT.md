# Driver Assist Development

## Run Tests
```
dotnet test
```

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

