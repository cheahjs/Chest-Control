## Chest-Control
Protect your chests.
Type command and open chest. Type the same command again to stop selecting.

### Usage:
_/cset_ - Lock the chest and sets you as owner - only you can open or destroy it ( You can use subcommand "public" to set you as owner, but everyone can open it. )

_/cunset_ - Removes protection of chest

_/crset_ - Sets region share on chest - everyone who can build in region can open chest, they can't destroy it

_/cpset_ - Sets chest public - everyone can open it, but they can't destroy it

_/clock_ _password_ - Sets password on chest - you can use _/clock_ _remove_ to disable password

_/ccset_ - Stops selecting of chest 

_/cunlock_  _password_ -  Unlocks and open chest

_/crefill_ - Sets chest to refill - _/crefill_ _remove_ to disable refill

### Permissions:
Everyone can use _/cunlock_ command.

_protectchest_ - standard permission for players - can protect its own chests

_openallchests_ - permission for admins - can open even protected chests, can't destroy them

_removechestprotection_ - permission for admins - can remove chests protections

_showchestinfo_ - permission for admins - gets info about opened chest

_refillchest_ - permission for admins - can set chests to refill

### Todo/Ideas:
- timed access - give player permission to open chest based on time and/or number of accesses
- integration with TShock user system
- use another format than txt files - use TShock DB?