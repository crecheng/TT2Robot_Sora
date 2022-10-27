﻿using robot.RobotModel;

namespace robot;

public static class RobotModelFactory
{
    private static Dictionary<string, Type> _allModel=new Dictionary<string, Type>()
    {
        {"突袭", typeof(RaidRobotModel)},
        {"简单回复",typeof(EasyReplyModel)},
        {"嘤嘤嘤",typeof(RandomImgModel)},
    };
    public static RobotModelBase GetModel(string name,GroupRobot group)
    {
        if (_allModel.TryGetValue(name, out Type t))
        {
            var model= Activator.CreateInstance(t) as RobotModelBase;
            model.Init(group.ID,group.RobotName);
            return model;
        }

        return null;
    } 
}