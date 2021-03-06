# -*- coding: utf-8 -*-
"""
Created on Sat Nov 17 19:04:12 2018

@author: Grant Pedersen and Devish Mundra

NOTES: https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Python-API.md
"""
import numpy as np
from model import RNN as unityAgent
from mlagents.envs import UnityEnvironment


episodes = 200
stateSize = 3 #envrionment state size (x position relative to target, z position relative to target, head to body position)
actionSize = 4 #(left = -1, up = 0, right = 1, down = 2)



def main():
    #file_name = None means to hook into the Unity editor
    #if there is a file it needs to be at root
    #otherwise the file name will look for a applicaiton.app or environment binary file.
    env = UnityEnvironment(file_name=None, worker_id=0, seed=1)
    print(str(env))
    agent1 = unityAgent(stateSize, actionSize, .99, 0.0005, 0.1, 0.999, 0.5, 25000, 200000)
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
                y = np.asarray([0,1,2,3])
                y = np.reshape(y, (1, 1, y.shape[0]))
                agent1.Train(X,y)
                action = agent1.Action(X) #need this to be an array
                action = np.column_stack([action])
                env_info = env.step(action)[mainBrain]
                #env_info = env.step(np.random.randn(len(env_info.agents), action_size[0]))[mainBrain]
                #print(env_info.vector_observations)
            else:
                action = np.column_stack([np.random.randint(0, action_size[i], size=(len(env_info.agents))) for i in range(len(action_size))])
                env_info = env.step(action)[mainBrain]
            episode_rewards += env_info.rewards[0]
            done = env_info.local_done[0]
    env.close()
            
main()
