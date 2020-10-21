using System;

namespace ASCReaderMC.PostProcessors {
	public class RandomTorchPostProcessor : MinecraftTerrainPostProcessor {

		public float chance;
		private Random random;

		public RandomTorchPostProcessor(float torchAmount) {
			chance = torchAmount;
			random = new Random();
		}

		public override void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z) {
			if(random.NextDouble() <= chance && region.IsAir(x, y + 1, z)) region.SetBlock(x, y + 1, z, "minecraft:torch");
		}
	}
}