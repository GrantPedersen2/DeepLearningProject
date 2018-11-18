# -*- coding: utf-8 -*-
"""
Created on Sat Nov 17 17:44:10 2018

@author: Grant
"""

import random
import numpy as np
import pandas as pd
from collections import deque
import keras
from keras import layers
from keras import models
import keras.backend as K

class MinimalRNNCell(keras.layers.Layer):

    def __init__(self, units, **kwargs):
        self.units = units
        self.state_size = units
        super(MinimalRNNCell, self).__init__(**kwargs)

    def build(self, input_shape):
        self.kernel = self.add_weight(shape=(input_shape[-1], self.units),
                                      initializer='uniform',
                                      name='kernel')
        self.recurrent_kernel = self.add_weight(
            shape=(self.units, self.units),
            initializer='uniform',
            name='recurrent_kernel')
        self.built = True

    def call(self, inputs, states):
        prev_output = states[0]
        h = K.dot(inputs, self.kernel)
        output = h + K.dot(prev_output, self.recurrent_kernel)
        return output, [output] 

class RNN:
    def __init__(self, state_size, action_size, gamma, learning_rate, epsilon, epsilon_decay, epsilon_min, epsilon_delay, memory_length):
        self.state_size = state_size
        self.action_size = action_size
        self.gamma = gamma
        self.learning_rate = learning_rate
        self.epsilon = epsilon                      # begining exploration rate  (e.g., 1.0)
        self.epsilon_decay = epsilon_decay          # rate of exploration decay  (e.g., .9999)
        self.epsilon_min = epsilon_min              # minimum exploration rate  (e.g., .05)
        self.epsilon_delay = epsilon_delay          # delay (n-frames) before exploration decay starts  (e.g., 25000)
        self.memory_length = memory_length          # size of replay memory
        self.memory = deque(maxlen=memory_length)   # replay memory array (tracks last n [s,a,r,s'] updates)
        self.model = self.BuildModel()
        self.target_model = self.BuildModel()
    
    def BuildModel(self, modelName = 'RNN'):
        model = models.Sequential()
        
        if modelName == 'RNN':
            cell = MinimalRNNCell(40)
            model.add(keras.layers.RNN(cell, return_sequences=True))
        else:
            model.add(layers.LSTM(45, return_sequences=True))
        
        model.add(layers.Activation('softmax'))
        
        model.compile(loss='mse', optimizer=keras.optimizers.Adam(lr=self.learning_rate))
        return model
    
    def Action(self, state):
        if np.random.rand() <= self.epsilon:
            return random.randrange(self.action_size)
        act_values = self.model.predict(state)
        return np.argmax(act_values[0])