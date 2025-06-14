﻿using AuctionHouseAPI.Application.DTOs.Update;
using AuctionHouseAPI.Domain.Models;

namespace AuctionHouseAPI.Application.Services.Interfaces
{
    public interface IAuctionService
    {
        public Task<int> CreateAuctionAsync(Auction auction);
        public void AddTagsToAuction(List<Tag> tags, Auction auction);
        public Task UpdateAuctionItemAsync(Auction auction, UpdateAuctionItemDTO updateAuctionItemDTO, int userId);
        public Task UpdateAuctionOptionsAsync(Auction auction, UpdateAuctionOptionsDTO updateAuctionOptionsDTO, int userId);
        public Task DeleteAuctionAsync(Auction auction, int userId);
    }
}
