We built a prototype where players can join a host, see each other moving, opening chests, dropping items from inventory (inventory can be opened with I, unfortunately items cannot be picked up yet).
All of this is done with LiteNetLib, no easy networking solution was used.
The code is really messy right now, but we plan to fix that later.

To test: open up two instances of the game, on one click 'Host Game', on the other one enter the ip (if on the same pc its 127.0.0.1) click 'Join Game', and everything should work.
