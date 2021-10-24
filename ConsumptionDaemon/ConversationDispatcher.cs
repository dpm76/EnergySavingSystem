using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Communication;
using Communication.Messages;
using ConsumptionModelling.Alerts;

namespace ConsumptionDaemon
{
    public class ConversationDispatcher:IConversationDispatcher
    {
        /// <summary>
        /// Trata el mensaje
        /// </summary>
        /// <param name="remoteEndPoint">Enlace remoto</param>
        /// <param name="message">Mensaje recibido</param>
        /// <param name="stream">Flujo con la entidad remota</param>
        public void Dispatch(IPEndPoint remoteEndPoint, IMessage message, NetworkStream stream)
        {
            NLog.LogManager.GetCurrentClassLogger().Trace("Mensaje recibido: " + message.ToString());

            if (message is GetAvailableConsumptionSourcesCommand)
            {
                DispatchGetAvailableConsumptionSourcesCommand(stream);
            }
            else if (message is RegisterConsumptionDataReaderListenerCommand)
            {
                DispatchRegisterConsumptionDataReaderListenerCommand((RegisterConsumptionDataReaderListenerCommand)message, remoteEndPoint);
            }
            else if (message is UnregisterConsumptionDataReaderListenerCommand)
            {
                DispathUnregisterConsumptionDataReaderListenerCommand((UnregisterConsumptionDataReaderListenerCommand) message, remoteEndPoint.Address);
            }else if(message is ModelDataQuery)
            {
                DispatchModelDataQuery((ModelDataQuery)message, stream);
            }else if(message is QHourConsumptionHistoricDataQuery)
            {
                DispatchQHourConsumptionHistoricDataQuery((QHourConsumptionHistoricDataQuery) message, stream);
            }else if(message is GetAlerts)
            {
                DispatchGetAlerts(message as GetAlerts, stream);
            }else if(message is DeleteAlert)
            {
                DispatchDeleteAlert(message as DeleteAlert);
            }else if(message is DeleteSourceAlerts)
            {
                DispatchDeleteSourceAlerts(message as DeleteSourceAlerts);
            }else if(message is GetConsumptionCosts)
            {
                DispatchGetConsumptionCosts(message as GetConsumptionCosts, stream);
            }
        }

        private static void DispatchGetConsumptionCosts(GetConsumptionCosts message, NetworkStream stream)
        {
            try
            {
                MessageSerializer.Instance.ToStream(stream,
                                                    new ConsumptionCostInfo
                                                        {
                                                            SourceId = message.SourceId,
                                                            Since = message.Since,
                                                            Until = message.Until,
                                                            CommitedPowerCost =
                                                                ConsumptionManager.Default.ConsumptionCostCalculator.
                                                                CommitedConsumptionCost(message.SourceId, message.Since,
                                                                                        message.Until),
                                                            EstimatedPowerCost =
                                                                ConsumptionManager.Default.ConsumptionCostCalculator.
                                                                EstimateCost(message.SourceId, message.Since,
                                                                             message.Until),
                                                            EstimatedPowerCostSaved =
                                                                ConsumptionManager.Default.ConsumptionCostCalculator.
                                                                EstimateSaving(message.SourceId, message.Since,
                                                                               message.Until)
                                                        });
            }catch(Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().WarnException(ex.ToString(), ex);
            }
        }

        private static void DispatchDeleteSourceAlerts(DeleteSourceAlerts message)
        {
            if(message.All)
            {
                AlertsContainer.Instance.ClearAll();
            }else
            {
                AlertsContainer.Instance.ClearSource(message.SourceId);
            }
        }

        private static void DispatchDeleteAlert(DeleteAlert message)
        {
            AlertsContainer.Instance.RemoveAlert(message.SourceId, message.TimeStamp);
        }

        private static void DispatchGetAlerts(GetAlerts message, NetworkStream stream)
        {
            if (message.All)
            {
                foreach (string sourceId in AlertsContainer.Instance.Alerts.Keys)
                {
                    MessageSerializer.Instance.ToStream(stream, new SourceAlerts
                                                                    {
                                                                        Empty = false,
                                                                        AlertTimeStamps = new List<DateTime>(AlertsContainer.Instance.Alerts[sourceId]),
                                                                        SourceId = sourceId
                                                                    });
                }
            }
            else if(AlertsContainer.Instance.Alerts.ContainsKey(message.SourceId))
            {
                MessageSerializer.Instance.ToStream(stream, new SourceAlerts
                                                                {
                                                                    Empty = false,
                                                                    AlertTimeStamps = new List<DateTime>(AlertsContainer.Instance.Alerts[message.SourceId]),
                                                                    SourceId = message.SourceId
                                                                });
            }

            MessageSerializer.Instance.ToStream(stream, new SourceAlerts
                                                            {
                                                                Empty = true
                                                            });
        }

        private static void DispatchGetAvailableConsumptionSourcesCommand(NetworkStream stream)
        {
            byte[] buffer = MessageSerializer.Instance.ToBuffer(
                   new AvailableConsumptionSourcesMessage
                   {
                       MagnitudesBySources = ConsumptionManager.Default.GetSourcesAndMagnitudes()
                   });
            stream.Write(buffer, 0, buffer.Length);
        }

        private static void DispatchRegisterConsumptionDataReaderListenerCommand(RegisterConsumptionDataReaderListenerCommand command,IPEndPoint remoteEndPoint)
        {
            ConsumptionManager.Default.RegisterListener(
                remoteEndPoint.Address,
                command.ListenPort,
                command.SourceId);
        }

        private static void DispathUnregisterConsumptionDataReaderListenerCommand(UnregisterConsumptionDataReaderListenerCommand command, IPAddress remoteIpAddress)
        {
            ConsumptionManager.Default.UnregisterListener(
                    remoteIpAddress,
                    command.SourceId);
        }

        private static void DispatchModelDataQuery(ModelDataQuery command, NetworkStream stream)
        {
            float[] data =
                    command.SaveModel
                        ? ConsumptionManager.Default.SavingModelDataProvider.GetData(command.SourceId, command.Since,
                                                                                     command.Until)
                        : ConsumptionManager.Default.PreviousModelDataProvider.GetData(command.SourceId, command.Since,
                                                                                      command.Until);
            ModelDataResponse response = new ModelDataResponse
                                             {
                                                 Data = (data != null) ? new List<float>(data) : null
                                             };

            MessageSerializer.Instance.ToStream(stream, response);
        }

        private static void DispatchQHourConsumptionHistoricDataQuery(QHourConsumptionHistoricDataQuery query, NetworkStream stream)
        {
            float?[] data = ConsumptionManager.Default.CommitedConsumptionDataProvider.GetData(query.SourceId,
                                                                                  query.Since,
                                                                                  query.Until);
            QHourConsumptionHistoricDataResponse response = new QHourConsumptionHistoricDataResponse
            {
                Data = new List<float?>(data)
            };
            MessageSerializer.Instance.ToStream(stream, response);
        }
    }
}
