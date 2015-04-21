# MakerRanger
Makerfaire 2015 project
Project written for Makerfaire UK 2015
Two cages contain an MIFARE RFID reader each. Stuffed animals with RFID tags embedded in them are placed on demand into
the cages to supposedly have a health scan. 
Two Arduinos are connected to the Netduino via I2C to control them.
An Arduino  is used to drive two wheel of fortune style displays using stepper motors that allows any onlookers to see
what the partipants need to find. 
An Arduino is used to drive adressablel RGB leds on each cage to add visual feedback and pretty displays.
Two push buttons are used by the "players" to move to the next round in the game. 
Two LCD displays 16x2 show game instructions. 
SD card in Netduino holds game configuration information.
WebServer in Netduino is used to allow twitter ID's to be scanned from barcodes and a separate app into the game. 
If a twitter ID has been used for a player, then the thermal label printed will show the twitter image for that profile. 
A thermal printer is attached (ZEBRA) to the serial port to print stickers off as a give away to participants. 
Game stats are recorded to SD for future reference. 
Parameters like the number of rounds and number of players is set using a set of RFID tags presented to the readers. 
Feel free to contact me if you have any questions via my blog http://www.timwappat.info/
Please note that some of the classes (RFID reader/Webserver) are adapted from othes work.
