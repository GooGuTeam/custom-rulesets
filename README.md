# custom-rulesets

This project produces patched rulesets to add ruleset id for [g0v0-server](https://github.com/GooGuTeam/g0v0-server)

## Rulesets

| Ruleset   | Repository                                                      | Short Name | Legacy ID |
| --------- | --------------------------------------------------------------- | ---------- | --------- |
| Sentakki  | [LumpBloom7/sentakki](https://github.com/LumpBloom7/sentakki)   | Sentakki   | 10        |
| tau       | [taulazer/tau](https://github.com/taulazer/tau)                 | tau        | 11        |
| Rush!     | [Beamographic/rush](https://github.com/Beamographic/rush)       | rush       | 12        |
| hishigata | [LumpBloom7/hishigata](https://github.com/LumpBloom7/hishigata) | hishigata  | 13        |
| soyokaze! | [goodtrailer/soyokaze](https://github.com/goodtrailer/soyokaze) | soyokaze   | 14        |

## Use CustomRulesetGenerator to generate metadata for rulesets

Run

```bash
dotnet run --
```

to see available options.

## License

CustomRulesetGenerator, all patches and workflows are licensed under the [AGPL-3.0 License](LICENSE).

Each ruleset has its own license. Refer to `ruleset-licenses/{ruleset}.txt` for details.
