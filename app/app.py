#!/usr/bin/env python2

import time
import os
from networkmanager import *
from room import *
from config import *
import sys

class App:
    def __init__(self):
        self.config = Config()
        openzwave_path = os.path.dirname(libopenzwave.__file__)
        self.nm = NetworkManager('/dev/ttyACM1', openzwave_path+"/config/")

        while not self.nm.is_ready:
            time.sleep(0.5)

        self.rooms = self.config.get_rooms()
        for node in self.nm.get_bulbs():
            self.rooms[self.config.get_room_for_node(node)].add_bulb(node)
            self.rooms["all"].add_bulb(node)

        self.scenes = self.config.get_scenes(self.rooms)
        self.scenes.movie()

app = App()
