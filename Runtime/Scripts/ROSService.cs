using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;


public abstract class ROSService
{
    public string ServiceName { get; protected set; }
    public ROSConnection Connection { get; set; }


    protected ROSService()
    {
    }


    public abstract void Init();
    public void Init(ROSConnection connection)
    {
        Connection = connection;
        Init();
    }

    public abstract void Call();
    public abstract void Call(Message request);
    // public abstract void Call(Message request, Action<Message> callback);
    
    public abstract void SetDefaultRequest(Message newDefaultRequest);
    public abstract Message GetDefaultRequest();

}

public class ROSServiceFactory
{
    public static ROSService<TReq, TResp> CreateROSService<TReq, TResp>(string serviceName, Message defaultReq) where TReq : Message where TResp : Message, new()
    {
        return new ROSService<TReq, TResp>(serviceName, defaultReq);
    }
}

public class ROSService<TReq,TResp> : ROSService where TReq : Message where TResp : Message, new()
{
    public TReq DefaultRequest { get; protected set; }

    public ROSService(string serviceName, Message defaultReq = null, ROSConnection connection = null)
    {
        ServiceName = serviceName;
        Connection = connection;
        DefaultRequest = defaultReq as TReq;
    }

    public override Message GetDefaultRequest()
    {
        return DefaultRequest;
    }


    public sealed override void Init()
    {
        if (!Connection || !Connection.HasConnectionThread)
        {
            var serviceManager = ROSServiceManager.Instance;
            if (serviceManager)
            {
                Connection = serviceManager.GetROSConnection();
            }
        }
        
        Connection.RegisterRosService<TReq, TResp>(ServiceName);
    }


    public void Call(TReq req, Action<TResp> callback)
    {
        if (Connection && Connection.HasConnectionThread)
        {
            Connection.SendServiceMessage<TResp>(ServiceName, req, callback);
        }
    }

    public void Call(Action<TResp> callback)
    {
        Call(DefaultRequest, callback);
    }
    
    public void Call(TReq req)
    {
        Call(req, DefaultCallback);
    }

    public override void Call(Message request)
    {
        Call(request as TReq, DefaultCallback);
    }

    public override void Call()
    {
        Call(DefaultRequest, DefaultCallback);
    }

    // public override void Call(Message request)
    // {
    //     Call(request as TReq, DefaultCallback);
    // }
    //
    // public override void Call(Message request, Action<Message> callback)
    // {
    //     Call(request as TReq, callback);
    // }

    void DefaultCallback(TResp resp)
    {
        Debug.Log($"Got an answer from {ServiceName}: {resp}");
    }
    
    
    public override void SetDefaultRequest(Message newDefaultRequest)
    {
        DefaultRequest = newDefaultRequest as TReq;
    }
}
