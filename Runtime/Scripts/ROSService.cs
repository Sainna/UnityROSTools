using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;

namespace Sainna.Robotics.ROSTools
{
    /// <summary>
    /// Abstract class representing a ROS Service.
    /// Used to store ROS services regardless of Request/Response type
    /// </summary>
    public abstract class ROSService
    {
        /// <summary>
        /// The service name, need to be the same as would be called in ROS
        /// </summary>
        public string ServiceName { get; protected set; }
        
        /// <summary>
        /// Reference to the current ROSConnection
        /// </summary>
        /// <remarks>
        /// todo: May or may not be useful to keep for each member
        /// </remarks>
        public ROSConnection Connection { get; set; }

        /// <summary>
        /// Constructor for the <see cref="ROSService"/> abstract class
        /// </summary>
        /// <seealso cref="ROSService{TReq,TResp}.ROSService(string,Message,ROSConnection)"/>
        protected ROSService()
        {
        }

        /// <seealso cref="ROSService{TReq,TResp}.Init()"/>
        public abstract void Init();
        
        /// <summary>
        /// Assigns a <see cref="Connection"/> before calling <see cref="Init()"/> on it.
        /// Used to create the service before the connection has been made.
        /// </summary>
        /// <param name="connection">The <see cref="ROSConnection"/> currently in use</param>
        /// <seealso cref="ROSService{TReq, TResp}.ROSService(string, Message, ROSConnection)"/>
        public void Init(ROSConnection connection)
        {
            Connection = connection;
            Init();
        }

        /// <summary>
        /// Calls the service with the default request and the default callback
        /// </summary>
        /// <seealso cref="ROSService{TReq, TResp}.Call()"/>
        public abstract void Call();

        /// <summary>
        /// Calls the service with a custom request
        /// </summary>
        /// <param name="request">The request you will send to the service (Type need to be the same as underlying <typeparamref name="TReq"/>)</param>
        /// <seealso cref="ROSService{TReq, TResp}.Call(TReq)"/>
        public abstract void Call(Message request);
        // public abstract void Call(Message request, Action<Message> callback);

        /// <summary>
        /// Changes the <see cref="ROSService{TReq,TResp}.DefaultRequest"/> of the service
        /// </summary>
        /// <param name="newDefaultRequest">The Request that will become the new custom request. Type needs to match the underlying service.</param>
        /// <seealso cref="ROSService{TReq, TResp}.SetDefaultRequest(Unity.Robotics.ROSTCPConnector.MessageGeneration.Message)"/>
        public abstract void SetDefaultRequest(Message newDefaultRequest);
        
        /// <summary>
        /// Get the <see cref="ROSService{TReq,TResp}.DefaultRequest"/> of the service
        /// </summary>
        /// <returns>The default request of the service</returns>
        /// <seealso cref="ROSService{TReq, TResp}.GetDefaultRequest()"/>
        public abstract Message GetDefaultRequest();

    }

    /// <summary>
    /// Provides a static function to create a generic <see cref="ROSService{TReq,TResp}"/>
    /// </summary>
    public class ROSServiceFactory
    {
        /// <summary>
        /// Get the <see cref="ROSService{TReq,TResp}.DefaultRequest"/> of the service
        /// </summary>
        /// <param name="serviceName">The ROS Service name (that will become the class <see cref="ROSService.ServiceName"/>)</param>
        /// <param name="defaultReq">The default requests of the service (Type need to be the same as <typeparamref name="TReq"/>)</param>
        /// <returns>The newly created service</returns>
        /// <typeparam name="TReq">The Request type (needs to be a child of type Message)</typeparam>
        /// <typeparam name="TResp">The Response type (needs to be a child of type Message)</typeparam>
        /// <seealso cref="ROSService{TReq, TResp}"/>
        public static ROSService<TReq, TResp> CreateROSService<TReq, TResp>(string serviceName, Message defaultReq)
            where TReq : Message where TResp : Message, new()
        {
            return new ROSService<TReq, TResp>(serviceName, defaultReq);
        }
    }

    /// <summary>
    /// Represents a ROS Service. Used to call the service with default or custom request and callback.
    /// </summary>
    /// <typeparam name="TReq">The Request type (needs to be a child of type Message)</typeparam>
    /// <typeparam name="TResp">The Response type (needs to be a child of type Message)</typeparam>
    public class ROSService<TReq, TResp> : ROSService where TReq : Message where TResp : Message, new()
    {
        public TReq DefaultRequest { get; protected set; }

        /// <summary>
        /// Assigns a <see cref="ROSService.Connection"/> before calling <see cref="Init()"/> on it.
        /// Used to create the service before the connection has been made.
        /// </summary>
        /// <param name="connection">The <see cref="ROSConnection"/> currently in use</param>
        /// <seealso cref="ROSService{TReq, TResp}.ROSService(string, Message, ROSConnection)"/>
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
                var serviceManager = ROSServiceManager.GetOrCreateInstance();
                if (serviceManager)
                {
                    
                    Connection = serviceManager.GetROSConnection();
                }
            }

            // For topic, just change this one
            Connection.RegisterRosService<TReq, TResp>(ServiceName);
        }

        /// <summary>
        /// Calls the service with a custom request and callback
        /// </summary>
        /// <param name="req">The request you will send to the service (Type need to be the same as underlying <typeparamref name="TReq"/>)</param>
        /// <param name="callback">The callback function that takes <typeparamref name="TResp"/> as a parameter</param>
        /// <example>
        /// <code>
        /// // Get a reference to the current Service Manager
        /// var service = ROSServiceManager.GetOrCreateInstance().GetService("dummy_service") as ROSService&lt;DummyRequest,DummyResponse&gt;;
        ///
        /// // Create your request object
        /// var request = new DummyRequest();
        ///
        /// // Call the service
        /// service.Call(request, ExampleCallback);
        ///
        /// void ExampleCallback(PlanPoseResponse resp)
        /// {
        ///     Debug.Log("Callback!");
        /// }
        /// </code>
        /// </example>
        public void Call(TReq req, Action<TResp> callback)
        {
            if (Connection && Connection.HasConnectionThread)
            {
                Connection.SendServiceMessage<TResp>(ServiceName, req, callback);
            }
        }

        /// <summary>
        /// Calls the service with a custom callback and the <see cref="DefaultRequest"/>
        /// </summary>
        /// <param name="callback">The callback function that takes <typeparamref name="TResp"/> as a parameter</param>
        /// <seealso cref="Call(TReq,System.Action{TResp})"/>
        public void Call(Action<TResp> callback)
        {
            Call(DefaultRequest, callback);
        }
        
        /// <summary>
        /// Calls the service with a custom request and the <see cref="DefaultCallback"/>
        /// </summary>
        /// <param name="req">The request you will send to the service (Type need to be the same as underlying <typeparamref name="TReq"/>)</param>
        /// <seealso cref="Call(TReq,System.Action{TResp})"/>
        public void Call(TReq req)
        {
            Call(req, DefaultCallback);
        }

        /// <summary>
        /// Calls the service with a custom request and the <see cref="DefaultCallback"/>, without needing to manually cast the request as the underlying <typeparamref name="TReq"/>.
        /// </summary>
        /// <param name="request">The request you will send to the service (Type need to be the same as underlying <typeparamref name="TReq"/>)</param>
        /// <seealso cref="Call(TReq,System.Action{TResp})"/>
        public override void Call(Message request)
        {
            Call(request as TReq, DefaultCallback);
        }

        /// <summary>
        /// Calls the service with the <see cref="DefaultRequest"/> and the <see cref="DefaultCallback"/>
        /// </summary>
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
}