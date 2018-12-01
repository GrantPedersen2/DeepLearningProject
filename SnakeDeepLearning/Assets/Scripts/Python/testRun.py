# -*- coding: utf-8 -*-
"""
Created on Tue Nov 20 12:49:44 2018

@author: Grant
"""

import numpy as np
from model import RNN as unityAgent
import keras
from mlagents.envs import UnityEnvironment

seed = 88759324
np.random.seed(seed)
episodes = 200
stateSize = 3 #envrionment state size (x position relative to target, z position relative to target, head to body position)
actionSize = 4
agent1 = unityAgent(stateSize, actionSize, .99, 0.0005, 0.01, 0.999, 0.5, 25000, 200000)

def LoadFile():
    with open(r"C:\Users\mundr\source\repos\GrantPedersen2\DeepLearningProject\SnakeDeepLearning\results.txt") as file:#\BestSet.txt") as file:
        dataSet = np.array([line.strip(" ,[]\n").split(',') for line in file.readlines()]).astype(float)
    X = dataSet[:, :-1]
    y = dataSet[:, -1]
    return X,y
    
def main():
    X, y = LoadFile()
    X = np.reshape(X, (1, X.shape[0], X.shape[1]))
    y = keras.utils.np_utils.to_categorical(y,num_classes=4)
    y = np.reshape(y, (1, y.shape[0], y.shape[1]))
#    for i in range(0,3):
    agent1.Train(X, y)
    testX = X[0, 0, :]
    testX = np.reshape(testX, (1, 1, testX.shape[0]))
    result = agent1.Action(testX)
    print(result)
    
def mainTwo():
    #file_name = None means to hook into the Unity editor
    #if there is a file it needs to be at root
    #otherwise the file name will look for a applicaiton.app or environment binary file.
    env = UnityEnvironment(file_name=None, worker_id=0, seed=1)
    print(str(env))
    mainBrain = env.brain_names[0]
    brain = env.brains[mainBrain]
    
    #Main loop
    for episode in range(episodes):
        env_info = env.reset(train_mode=True)[mainBrain]
        done = False
        episode_rewards = 0
        while not done:
            action_size = brain.vector_action_space_size
            if brain.vector_action_space_type == 'continuous':
                X = np.asarray(env_info.vector_observations)
                X = np.reshape(X, (1, X.shape[0], X.shape[1]))
                action = agent1.Action(X)
                action = np.column_stack([action])
                env_info = env.step(action)[mainBrain]
            else:
                action = np.column_stack([np.random.randint(0, action_size[i], size=(len(env_info.agents))) for i in range(len(action_size))])
                env_info = env.step(action)[mainBrain]
            episode_rewards += env_info.rewards[0]
            done = env_info.local_done[0]
    env.close()

main()
mainTwo()