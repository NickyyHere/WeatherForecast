﻿using AuctionHouseAPI.Application.Services.Interfaces;
using AuctionHouseAPI.Domain.Interfaces;
using AuctionHouseAPI.Domain.Models;
using AuctionHouseAPI.Shared.Exceptions;

namespace AuctionHouseAPI.Application.Services
{
    public class BidService : IBidService
    {
        private readonly IBidRepository _bidRepository;
        public BidService(IBidRepository bidRepository)
        {
            _bidRepository = bidRepository;
        }

        public async Task CreateBidAsync(Bid bid, Auction auction, int userId)
        {
            if (auction.Options!.StartDateTime > DateTime.UtcNow || auction.Options.FinishDateTime < DateTime.UtcNow)
            {
                throw new InactiveAuctionException($"Can't place bid on inactive auction");
            }
            if (auction.OwnerId == userId)
            {
                throw new BidOnOwnedAuctionException("Bidding on your own auction is not allowed");
            }
            var highestBid = await _bidRepository.GetHighestAuctionBidAsync(bid.AuctionId);
            var minimumRequired = highestBid == null ? auction.Options.StartingPrice : highestBid.Amount + auction.Options.MinimumOutbid;
            if (bid.Amount < minimumRequired)
            {
                throw new MinimumOutbidException($"Minimum outbid is {auction.Options.MinimumOutbid}, {minimumRequired} to reach the minimum.");
            }
            
            bid.UserId = userId;
            await _bidRepository.CreateAsync(bid);
            await _bidRepository.CommitTransactionAsync();
        }

        public async Task WithdrawFromAuctionAsync(Auction auction, int userId)
        {
            await _bidRepository.BeginTransactionAsync();
            try
            {
                if (auction.Options!.FinishDateTime < DateTime.UtcNow)
                {
                    throw new FinishedAuctionException();
                }
                var userAuctionBids = await _bidRepository.GetByUserAndAuctionAsync(userId, auction.Id);
                foreach (var bid in userAuctionBids)
                {
                    await _bidRepository.DeleteAsync(bid);
                }
                await _bidRepository.CommitTransactionAsync();
            }
            catch
            {
                await _bidRepository.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
