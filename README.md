# Charles B

Make BattleTech mechs fall down when they miss because that's funny.

<img src="https://media.giphy.com/media/Ou18ZgE49Fss0/giphy.gif" />

##
BattleTech Mod (using [BTML](https://github.com/Mpstark/BattleTechModLoader) and [ModTek](https://github.com/Mpstark/ModTek)) that adds instability/knockdown when a melee attack misses.

## Features

- when you are melee'ing and you miss, add some instability to yourself
- when you are melee'ing and you're legged, add a different amount of instability
- knock yourself down if you add enough instability, even if you're not unsteady before the attack 

## Download
Downloads can be found on [Github](https://github.com/janxious/CharlesB/releases).

## Install
- [Install BTML and Modtek](https://github.com/Mpstark/ModTek/wiki/The-Drop-Dead-Simple-Guide-to-Installing-BTML-&-ModTek-&-ModTek-mods).
- Make sure the `CharlesB.dll` and `mod.json` files into `\BATTLETECH\Mods\CharlesB` folder.
- Start the game.

## Settings

Setting | Type | Default | Description
--- | --- | --- | ---
`attackMissInstabilityPercent` | `int` | `40` | the percentage of your stability bar that is filled when a mech misses a melee attack
`attackMissInstabilityLeggedPercent` | `int` | `100` | the percentage of your stability bar that is filled when the attacking mech is legged and misses a melee attack
`allowSteadyToKnockdown` | `bool` | `true` | allow a mech to go from steady to knockdown as part of melee attack miss
`debug` | `bool` | `false` | enable debugging logs, probably not useful unless you are changing the code or looking at it as you run the mod

## Special Thanks

HBS, @Mpstark, @Morphyum