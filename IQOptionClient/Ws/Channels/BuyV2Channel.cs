﻿using System;
using System.Reactive.Linq;
using System.Reactive.Observable.Aliases;
using IQOptionClient.Utilities;
using IQOptionClient.Ws.Channels.Abstractions;
using IQOptionClient.Ws.Channels.Bases;
using IQOptionClient.Ws.Exceptions;
using IQOptionClient.Ws.Models;

namespace IQOptionClient.Ws.Channels
{
    public class BuyV2Channel : IChannelPublisher<BuyInputModel, BuyServerModel>
    {
        private const string ChannelName = "buyV2";

        private readonly IWsIQClient _wsIqClient;
        private readonly IEpoch _epoch;
        private readonly IqOptionGenericChannelPublisher<dynamic> _publisher;


        public BuyV2Channel(IWsIQClient wsIqClient, IEpoch epoch)
        {
            _wsIqClient = wsIqClient;
            _epoch = epoch;
            _publisher = new IqOptionGenericChannelPublisher<dynamic>(wsIqClient, ChannelName);
        }

        public IObservable<BuyServerModel> SendMessage(BuyInputModel message)
        {
            return _wsIqClient.ServerDatetime
                .FirstAsync()
                .FlatMap(serverTime =>
                {
                    var expirationMinutes = 1;
                    if (serverTime.Second > 30)
                    {
                        expirationMinutes++;
                    }

                    var expirationTime = new DateTime(serverTime.Year, serverTime.Month, serverTime.Day, serverTime.Hour, serverTime.Minute, 0).AddMinutes(expirationMinutes);
                    var expirationUnixTime = _epoch.SecondsUnixTimeFromDateTime(expirationTime);

                    var serverInputMessage = new BuyServerModel(message.Price, message.Active, message.Direction, expirationUnixTime, _epoch.EpochSeconds, "turbo");


                    return _publisher.SendMessage(serverInputMessage).FlatMap(Observable.Return(serverInputMessage));
                });
        }

        public void Dispose()
        {
        }
    }
}
