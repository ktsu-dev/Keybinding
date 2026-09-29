## v2.1.3 (patch)

Changes since v2.1.2:

- Comment the intentionally empty activation loop in the race test ([@Claude](https://github.com/Claude))
- Lock SetActiveProfile and DeleteProfile so a delete cannot leave a stale active id [patch] ([@Claude](https://github.com/Claude))
- Skip invalid stored profiles and chords instead of failing the load [patch] ([@Claude](https://github.com/Claude))

