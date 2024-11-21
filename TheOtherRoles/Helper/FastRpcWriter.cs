using System;
using Hazel;
using InnerNet;
using UnityEngine;

namespace TheOtherRoles.Helper;

#nullable enable
public class FastRpcWriter : IDisposable
{
    private readonly RPCSendMode _rpcSendMode;
    private byte CallId;

    private int msgCount;

    private SendOption Option;

    private int SendTargetId;

    private uint targetObjectId;

    private MessageWriter? writer;

    public void Dispose()
    {
        Clear();
    }

    private FastRpcWriter(SendOption option, RPCSendMode mode = RPCSendMode.SendToAll, int TargetId = -1,
        uint ObjectId = 255)
    {
        Option = option;
        _rpcSendMode = mode;
        SetTargetId(TargetId);
        SetTargetObjectId(ObjectId);
    }

    private FastRpcWriter()
    {
    }

    private static FastRpcWriter StartNew(SendOption option = SendOption.Reliable,
        RPCSendMode mode = RPCSendMode.SendToAll, int TargetId = -1, uint targetObjectId = 255)
    {
        var writer = new FastRpcWriter(option, mode, TargetId, targetObjectId);
        writer.CreateWriter();
        return writer;
    }

    internal static FastRpcWriter StartNewRpcWriter(CustomRPC rpc, SendOption option = SendOption.Reliable,
        RPCSendMode mode = RPCSendMode.SendToAll, int TargetId = -1, uint targetObjectId = 255)
    {
        var writer = StartNew(option, mode, TargetId, targetObjectId);
        writer.SetRpcCallId(rpc);

        if (mode == RPCSendMode.SendToAll)
            writer.StartDataAllMessage();

        if (mode == RPCSendMode.SendToPlayer)
            writer.StartDataToPlayerMessage();

        writer.StartRPCMessage();
        return writer;
    }

    internal static FastRpcWriter StartNewRpcWriter(CustomRPC rpc, InnerNetObject obNetObject,
        SendOption option = SendOption.Reliable,
        RPCSendMode mode = RPCSendMode.SendToAll, int TargetId = -1)
    {
        var writer = StartNewRpcWriter(rpc, option, mode, TargetId, obNetObject.NetId);
        return writer;
    }

    public FastRpcWriter CreateWriter()
    {
        Clear();
        writer = MessageWriter.Get(Option);
        return this;
    }

    public FastRpcWriter StartSendAllRPCWriter()
    {
        CreateWriter();
        StartDataAllMessage();
        StartRPCMessage();
        return this;
    }

    public FastRpcWriter StartSendToPlayerRPCWriter()
    {
        CreateWriter();
        StartDataToPlayerMessage();
        StartRPCMessage();
        return this;
    }

    public FastRpcWriter SetSendOption(SendOption option)
    {
        Option = option;
        return this;
    }

    public FastRpcWriter SetTargetObjectId(uint id)
    {
        if (id == 255)
        {
            targetObjectId = PlayerControl.LocalPlayer.NetId;
            return this;
        }

        targetObjectId = id;
        return this;
    }

    public FastRpcWriter SetRpcCallId(CustomRPC id)
    {
        CallId = (byte)id;
        return this;
    }

    public FastRpcWriter SetRpcCallId(byte id)
    {
        CallId = id;
        return this;
    }

    public FastRpcWriter SetTargetId(int id)
    {
        if (id == -1)
            return this;

        SendTargetId = id;
        return this;
    }

    public void Clear()
    {
        if (writer == null) return;
        Recycle();
        writer = null;
    }

    public FastRpcWriter Write(bool value)
    {
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(int value)
    {
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(float value)
    {
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(string value)
    {
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(byte value)
    {
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(byte[] value, bool writeLength = false)
    {
        if (writeLength)
            writer?.WritePacked(value.Length);
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(Vector2 value)
    {
        writer?.Write(value.x);
        writer?.Write(value.y);
        return this;
    }

    public FastRpcWriter Write(Vector3 value)
    {
        writer?.Write(value.x);
        writer?.Write(value.y);
        writer?.Write(value.z);
        return this;
    }

    public FastRpcWriter Write(Rect value)
    {
        writer?.Write(value.x);
        writer?.Write(value.y);
        writer?.Write(value.width);
        writer?.Write(value.height);
        return this;
    }

    public FastRpcWriter Write(PlayerControl value)
    {
        writer?.Write(value.PlayerId);
        return this;
    }

    public FastRpcWriter Write(MessageWriter value, bool includeHeader)
    {
        writer?.Write(value, includeHeader);
        return this;
    }

    public FastRpcWriter Write(Version version)
    {
        writer?.Write(version.Major);
        writer?.Write(version.Minor);
        writer?.Write(version.Build);
        writer?.Write(version.Revision);
        return this;
    }


    public FastRpcWriter WriteWriter(MessageWriter value, bool includeHeader)
    {
        Write(value.ToByteArray(includeHeader), true);
        return this;
    }

    public FastRpcWriter Write(Il2CppStructArray<byte> value, bool writeLength = false)
    {
        if (writeLength)
            writer?.WritePacked(value.Length);
        writer?.Write(value);
        return this;
    }

    public FastRpcWriter Write(Il2CppStructArray<byte> value, int offset, int length)
    {
        writer?.Write(value, offset, length);
        return this;
    }

    public FastRpcWriter Write(params object[]? objects)
    {
        if (objects == null) return this;

        foreach (var obj in objects)
            switch (obj)
            {
                case byte _byte:
                    Write(_byte);
                    break;

                case string _string:
                    Write(_string);
                    break;

                case float _float:
                    Write(_float);
                    break;

                case int _int:
                    Write(_int);
                    break;

                case bool _bool:
                    Write(_bool);
                    break;

                case byte[] _bytes:
                    Write(_bytes);
                    break;
            }

        return this;
    }

    public FastRpcWriter WritePacked(int value)
    {
        writer?.WritePacked(value);
        return this;
    }

    public FastRpcWriter WritePacked(uint value)
    {
        writer?.WritePacked(value);
        return this;
    }

    private void StartDataAllMessage()
    {
        StartMessage((byte)_rpcSendMode);
        Write(AmongUsClient.Instance.GameId);
    }

    private void StartDataToPlayerMessage()
    {
        StartMessage((byte)_rpcSendMode);
        Write(AmongUsClient.Instance.GameId);
        WritePacked(SendTargetId);
    }

    private void StartRPCMessage()
    {
        StartMessage(2);
        WritePacked(targetObjectId);
        Write(CallId);
    }

    public FastRpcWriter StartMessage(byte flag)
    {
        writer?.StartMessage(flag);
        msgCount++;
        return this;
    }

    public FastRpcWriter EndMessage()
    {
        writer?.EndMessage();
        msgCount--;
        return this;
    }

    public void EndAllMessage()
    {
        while (msgCount > 0)
            EndMessage();
    }

    public void Recycle()
    {
        writer?.Recycle();
    }

    public void RPCSend()
    {
        EndAllMessage();
        AmongUsClient.Instance.SendOrDisconnect(writer);
        Recycle();
    }

    public static implicit operator MessageWriter(FastRpcWriter writer)
    {
        return writer.writer ??= MessageWriter.Get(writer.Option);
    }

    public static implicit operator FastRpcWriter(MessageWriter writer)
    {
        return new FastRpcWriter { writer = writer, Option = writer.SendOption };
    }
}

public enum RPCSendMode
{
    SendToAll = 5,
    SendToPlayer = 6
}