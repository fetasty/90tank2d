// # define MSG_LISTEN_LOG
// # define MSG_CANCEL_LISTEN_LOG
// # define MSG_SEND_LOG

using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void Callback(); // 无参委托
public delegate void Callback<T>(T arg); // 单参数委托
public delegate void Callback<T, K>(T arg1, K arg2); // 双参数委托
public delegate void Callback<T, K, E>(T arg1, K arg2, E arg3); // 三参数委托

public class Messager
{
    public class MessagerException : Exception {
        public MessagerException(string msg) : base(msg) {}
    }
    public static Messager Instance { get; } = new Messager();
    private Dictionary<MessageID, Delegate> messageDic = new Dictionary<MessageID, Delegate>();
    private Messager() {}
    public void Listen(MessageID messageID, Callback callback) {
        OnListening(messageID, callback);
        messageDic[messageID] = (Callback) messageDic[messageID] + callback;
    }
    public void Listen<T>(MessageID messageID, Callback<T> callback) {
        OnListening(messageID, callback);
        messageDic[messageID] = (Callback<T>) messageDic[messageID] + callback;
    }
    public void Listen<T, K>(MessageID messageID, Callback<T, K> callback) {
        OnListening(messageID, callback);
        messageDic[messageID] = (Callback<T, K>) messageDic[messageID] + callback;
    }
    public void Listen<T, K, E>(MessageID messageID, Callback<T, K, E> callback) {
        OnListening(messageID, callback);
        messageDic[messageID] = (Callback<T, K, E>) messageDic[messageID] + callback;
    }
    private void OnListening(MessageID messageID, Delegate handler) {
        if (!messageDic.ContainsKey(messageID)) {
            messageDic.Add(messageID, null);
        }
        Delegate d = messageDic[messageID];
        if (d != null && d.GetType() != handler.GetType()) {
            throw new MessagerException($"Attempting to add a listener with type {handler.GetType()} but current listeners type is {d.GetType()}");
        }
        # if MSG_LISTEN_LOG
        Debug.Log($"Message listen ID = {messageID}, type = {handler.GetType()}");
        # endif
    }
    public void CancelListen(MessageID messageID, Callback callback) {
        OnCancelListen(messageID, callback);
        messageDic[messageID] = (Callback) messageDic[messageID] - callback;
        OnCanceledListen(messageID);
    }
    public void CancelListen<T>(MessageID messageID, Callback<T> callback) {
        OnCancelListen(messageID, callback);
        messageDic[messageID] = (Callback<T>) messageDic[messageID] - callback;
        OnCanceledListen(messageID);
    }
    public void CancelListen<T, K>(MessageID messageID, Callback<T, K> callback) {
        OnCancelListen(messageID, callback);
        messageDic[messageID] = (Callback<T, K>) messageDic[messageID] - callback;
        OnCanceledListen(messageID);
    }
    public void CancelListen<T, K, E>(MessageID messageID, Callback<T, K, E> callback) {
        OnCancelListen(messageID, callback);
        messageDic[messageID] = (Callback<T, K, E>) messageDic[messageID] - callback;
        OnCanceledListen(messageID);
    }
    private void OnCancelListen(MessageID messageID, Delegate handler) {
        if (messageDic.ContainsKey(messageID)) {
            Delegate d = messageDic[messageID];
            if (d == null) {
                throw new MessagerException($"Attempting to remove listener for ID {messageID} but current listeners is null.");
            } else if (d.GetType() != handler.GetType()) {
                throw new MessagerException($"Attempting to remove listener with type {handler.GetType()} but current listeners type is {d.GetType()}");
            }
            # if MSG_CANCEL_LISTEN_LOG
            Debug.Log($"Message cancel listen ID = {messageID}, type = {handler.GetType()}");
            # endif
        } else {
            throw new MessagerException($"Attempting to remove listener for ID {messageID} but Messenger doesn't know about this event type.");
        }
    }
    private void OnCanceledListen(MessageID messageID) {
        if (messageDic[messageID] == null) {
            messageDic.Remove(messageID);
        }
    }
    public void Send(MessageID messageID) {
        if (messageDic.ContainsKey(messageID)) {
            Delegate d = messageDic[messageID];
            if (d.GetType() != typeof(Callback)) {
                throw new MessagerException($"Attempting to send message for ID {messageID} type{typeof(Callback)} but listeners type is {d.GetType()}.");
            }
            # if MSG_SEND_LOG
            Debug.Log($"Message send ID = {messageID}, type = {d.GetType()}");
            # endif
            ((Callback) d)();
        }
    }
    public void Send<T>(MessageID messageID, T arg) {
        if (messageDic.ContainsKey(messageID)) {
            Delegate d = messageDic[messageID];
            if (d.GetType() != typeof(Callback<T>)) {
                throw new MessagerException($"Attempting to send message for ID {messageID} type{typeof(Callback<T>)} but listeners type is {d.GetType()}.");
            }
            # if MSG_SEND_LOG
            Debug.Log($"Message send ID = {messageID}, type = {d.GetType()}");
            # endif
            ((Callback<T>) d)(arg);
        }
    }
    public void Send<T, K>(MessageID messageID, T arg1, K arg2) {
        if (messageDic.ContainsKey(messageID)) {
            Delegate d = messageDic[messageID];
            if (d.GetType() != typeof(Callback<T, K>)) {
                throw new MessagerException($"Attempting to send message for ID {messageID} type{typeof(Callback<T, K>)} but listeners type is {d.GetType()}.");
            }
            # if MSG_SEND_LOG
            Debug.Log($"Message send ID = {messageID}, type = {d.GetType()}");
            # endif
            ((Callback<T, K>) d)(arg1, arg2);
        }
    }
    public void Send<T, K, E>(MessageID messageID, T arg1, K arg2, E arg3) {
        if (messageDic.ContainsKey(messageID)) {
            Delegate d = messageDic[messageID];
            if (d.GetType() != typeof(Callback<T, K, E>)) {
                throw new MessagerException($"Attempting to send message for ID {messageID} type{typeof(Callback<T, K, E>)} but listeners type is {d.GetType()}.");
            }
            # if MSG_SEND_LOG
            Debug.Log($"Message send ID = {messageID}, type = {d.GetType()}");
            # endif
            ((Callback<T, K, E>) d)(arg1, arg2, arg3);
        }
    }
}
