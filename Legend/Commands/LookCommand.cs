﻿using System;
using System.Linq;
using Legend.Extensions;
using Legend.Models;

namespace Legend.Commands
{
    [CommandAttribute("look","Players look at the current room")]
    public class LookCommand : PlayerCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, Player callingPlayer, string[] args)
        {
            
            var room = context.World.GetRoom(callingPlayer.RoomReference);
            var players = context.World.GetOnlinePlayers(room).Where(player => player.Name != callingPlayer.Name).ToList();

            if (args.IsNullOrEmpty()) // Look room
            {
                var look = new PlayerMessages();

                look.Messages.Add(new PlayerMessage(room.Name, MessageType.Title));

                if (!callingPlayer.BriefDescriptions)
                    look.Messages.Add(new PlayerMessage(String.Format("   {0}", room.Description)));

                if (room.Items.Any())
                    look.Messages.Add(
                        new PlayerMessage(String.Format("There's {0} laying on the ground.", room.Items.ToDisplay())));

                if (players.Any())
                    look.Messages.Add(new PlayerMessage(players.ToDisplay(), MessageType.Important));

                context.NotificationService.ToPlayer(callingPlayer, look);
            }
            else // Look at item or player
            {
                var lookFor = String.Join(" ", args).Trim();

                if (callingPlayer.Name.StartsWith(lookFor, StringComparison.OrdinalIgnoreCase)) // This player
                    context.NotificationService.ToPlayer(callingPlayer, new PlayerMessages(callingPlayer.Description));
                else if (players.Count(x => x.Name.StartsWith(lookFor, StringComparison.OrdinalIgnoreCase)) > 0) // Other player
                {
                    var target = players.First(x => x.Name.StartsWith(lookFor, StringComparison.OrdinalIgnoreCase));

                    context.NotificationService.ToPlayer(callingPlayer, new PlayerMessages(target.Description));

                    context.NotificationService.ToPlayer(target, new PlayerMessages(String.Format("{0} is looking you over.", callingPlayer.Name)));

                    context.NotificationService.ToPlayers(players.Where(x => x.Name != target.Name),
                                                          new PlayerMessages(String.Format("{0} is taking a good look at {1}.",
                                                                                           callingPlayer.Name,target.Name)));
                }
                else if (callingPlayer.Items.Count(x => x.Name.StartsWith(lookFor, StringComparison.OrdinalIgnoreCase)) > 0) // Item in inventory
                {
                    context.NotificationService.ToPlayer(callingPlayer, new PlayerMessages(
                                                                            callingPlayer.Items.First(
                                                                                x =>
                                                                                x.Name.StartsWith(lookFor,
                                                                                                  StringComparison.
                                                                                                      OrdinalIgnoreCase))
                                                                                .Description));
                }
                else if (room.Items.Count(x => x.Name.StartsWith(lookFor, StringComparison.OrdinalIgnoreCase)) > 0) // Item in room
                {
                    context.NotificationService.ToPlayer(callingPlayer, new PlayerMessages(
                                                                            room.Items.First(
                                                                                x =>
                                                                                x.Name.StartsWith(lookFor,
                                                                                                  StringComparison.
                                                                                                      OrdinalIgnoreCase))
                                                                                .Description));
                }
                else
                {
                    context.NotificationService.ToPlayer(callingPlayer,
                                                         new PlayerMessages(
                                                             "...You don't see anything like that around here"));
                }
            }
        }
    }
}