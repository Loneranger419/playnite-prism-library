using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrismLibrary.Models;
using PrismLibrary.Services.Metadata;
using Xunit;

namespace PrismLibrary.Tests
{
    public class PackMetadataMapperTests
    {
        [Fact]
        public void ModrinthParse_MapsDescriptionCoverBackgroundAuthorsAndLinks()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "modrinth-project.json"));
            var info = ModrinthMetadataClient.Parse(json, "AABBCCDD");
            var instance = new PrismInstance
            {
                MinecraftVersion = "1.20.1",
                Loaders = new List<string> { "Fabric" },
                ManagedPack = true,
                ManagedPackType = "modrinth",
                ManagedPackId = "AABBCCDD"
            };
            var metadata = PackMetadataMapper.ToGameMetadata(info, instance);

            Assert.Contains("Full Cobblemon body", metadata.Description);
            Assert.Contains("Watch on YouTube", metadata.Description);
            Assert.DoesNotContain("<iframe", metadata.Description);
            Assert.Equal("https://cdn.modrinth.com/data/icon.png", metadata.CoverImage.Path);
            Assert.Equal("https://cdn.modrinth.com/data/gallery1.png", metadata.BackgroundImage.Path);
            Assert.Contains(metadata.Developers, d => d.ToString() == "Cobblemon Team");
            Assert.Contains(metadata.Links, l => l.Url.Contains("modrinth.com/modpack/cobblemon"));
            Assert.Contains(metadata.Links, l => l.Name == "YouTube");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Minecraft");
            Assert.Contains(metadata.Tags, t => t.ToString() == "MC 1.20.1");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Fabric");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Modrinth");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Adventure");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Magic");
        }

        [Fact]
        public void CurseForgeParse_MapsDescriptionCoverAndAuthors()
        {
            var modJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "curseforge-mod.json"));
            var descJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "curseforge-description.json"));
            var info = CurseForgeMetadataClient.Parse(modJson, descJson, "925200");
            var instance = new PrismInstance
            {
                MinecraftVersion = "1.21.1",
                Loaders = new List<string> { "NeoForge" },
                ManagedPack = true,
                ManagedPackType = "flame",
                ManagedPackId = "925200"
            };
            var metadata = PackMetadataMapper.ToGameMetadata(info, instance);

            Assert.Equal("<p>Full ATM10 description</p>", metadata.Description);
            Assert.Equal("https://media.forgecdn.net/avatars/thumb-atm10.png", metadata.CoverImage.Path);
            Assert.Equal("https://media.forgecdn.net/screenshots/atm10.png", metadata.BackgroundImage.Path);
            Assert.Contains(metadata.Developers, d => d.ToString() == "ATMTeam");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Minecraft");
            Assert.Contains(metadata.Tags, t => t.ToString() == "NeoForge");
            Assert.Contains(metadata.Tags, t => t.ToString() == "CurseForge");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Multiplayer");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Adventure and RPG");
            Assert.DoesNotContain(metadata.Tags, t => t.ToString() == "Modpacks");
        }

        [Fact]
        public void FtbParse_MapsSquareSplashDescriptionAndAuthors()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "ftb-modpack.json"));
            var info = FtbMetadataClient.Parse(json, "130");
            var instance = new PrismInstance
            {
                MinecraftVersion = "1.21.0",
                Loaders = new List<string> { "NeoForge" },
                ManagedPack = true,
                ManagedPackType = "ftb",
                ManagedPackId = "130"
            };
            var metadata = PackMetadataMapper.ToGameMetadata(info, instance);

            Assert.Contains("<h2", metadata.Description);
            Assert.Contains("<strong>The Stone Remembers</strong>", metadata.Description);
            Assert.Contains("<strong>StoneBlock 4</strong>", metadata.Description);
            Assert.DoesNotContain("## ", metadata.Description);
            Assert.Equal("https://cdn.feed-the-beast.com/square.png", metadata.CoverImage.Path);
            Assert.Equal("https://cdn.feed-the-beast.com/splash.png", metadata.BackgroundImage.Path);
            Assert.Contains(metadata.Developers, d => d.ToString() == "FTB Team");
            Assert.Contains(metadata.Links, l => l.Name == "Wiki");
            Assert.Contains(metadata.Tags, t => t.ToString() == "FTB");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Skyblock");
            Assert.Contains(metadata.Tags, t => t.ToString() == "Kitchen Sink");
            Assert.Contains(metadata.Tags, t => t.ToString() == "MC 1.21.0");
        }
    }
}
