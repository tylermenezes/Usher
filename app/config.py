#!/usr/bin/env python2

import sys
import yaml
from room import Room

sys.path.insert(0, r'../config/')
import scenes
import switches

class Config:
    def __init__(self):
        with open('../config/topo.yaml') as f:
            self.topo = yaml.safe_load(f)

    def get_rooms(self):
        r = {}
        for room in self.topo['rooms']:
            r[room] = Room(room)
        r["default"] = Room("Default room")
        r["all"] = Room("All rooms")
        return r

    def get_room_for_node(self, lookup):
        for room in self.topo['rooms']:
            for node in self.topo['rooms'][room]['nodes']:
                if node['id'] == lookup.id:
                    return room
        return "default"

    def get_scenes(self, rooms):
        return scenes.Scenes(rooms)
