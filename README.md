# custom-rulesets

This project produces patched rulesets to add ruleset id for [g0v0-server](https://github.com/GooGuTeam/g0v0-server)

## Rulesets

| Ruleset   | Repository                                                      | Short Name | Legacy ID |
|-----------|-----------------------------------------------------------------|------------|-----------|
| Sentakki  | [LumpBloom7/sentakki](https://github.com/LumpBloom7/sentakki)   | Sentakki   | 10        |
| tau       | [taulazer/tau](https://github.com/taulazer/tau)                 | tau        | 11        |
| Rush!     | [Beamographic/rush](https://github.com/Beamographic/rush)       | rush       | 12        |
| hishigata | [LumpBloom7/hishigata](https://github.com/LumpBloom7/hishigata) | hishigata  | 13        |
| soyokaze! | [goodtrailer/soyokaze](https://github.com/goodtrailer/soyokaze) | soyokaze   | 14        |

## Releases

Pushing a tag (for example `2026.912.1`) builds every ruleset and publishes the DLLs as a GitHub release.

The tag is stamped into the built assemblies, so `Assembly.GetName().Version` returns `{tag}.0`
(for example `2026.912.1.0`) and `AssemblyInformationalVersion` returns `{tag}+{upstream commit}`.
Builds which are not triggered by a tag (scheduled runs and pushes touching `patches/**`) are stamped
with the date-based version of the day instead (for example `2026.912.0`) and are not released.

## Use CustomRulesetMetadataGenerator to generate metadata for rulesets

Run

```bash
dotnet run --
```

to see available options.

## License

CustomRulesetMetadataGenerator, all patches and workflows are licensed under the [AGPL-3.0 License](LICENSE).

Each ruleset has its own license. Refer to `ruleset-licenses/{ruleset}.txt` for details.
