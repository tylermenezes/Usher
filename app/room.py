#!/usr/bin/env python2

import time
import math
import decimal
import thread
import ctypes
from threading import Thread
import colorsys
import random

class BulbAccess:
    def __init__(self, bulbs):
        self.bulbs = bulbs

    def __getattr__(self, name):
        def recursive_accessor(*args):
            for bulb in self.bulbs:
                getattr(bulb, name)(*args)
                time.sleep(0.2)

        return recursive_accessor

class Room:
    def _scene(fn):
        def newFn(self, *args):
            if hasattr(self, '_running_scene') and self._running_scene.isAlive():
                ctypes.pythonapi.PyThreadState_SetAsyncExc(ctypes.c_long(self._running_scene.ident), ctypes.py_object(SystemExit))

            self._running_scene = Thread(target=fn, args=(self,)+args)
            self._running_scene.start()
        return newFn

    def __init__(self, name):
        self.name = name
        self.bulbs = BulbAccess([])

    def add_bulb(self, bulb):
        self.bulbs.bulbs.append(bulb)
