﻿using ASCReader;
using ASCReader.Export;
using ASCReader.Util;
using System;
using System.Collections.Generic;

namespace ASCReaderMC {

	[PluginInfo("Minecraft Region Exporter")]
	public class MCRegionExportHandler : ASCReaderExportHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("mcaoffset", "X Z", "[MCA] Apply offset to region terrain, in regions (512)", this));
			list.Add(new ConsoleCommand("mcasplatmapper", "", "[MCA] Use splatmap files to define the world's surface (file <name>.splat required)", this));
		}

		public override void HandleCommand(string cmd, string[] args, ExportOptions options, ASCData data) {
			if(cmd == "mcaoffset") {
				if(args.Length > 1) {
					bool b = true;
					b &= int.TryParse(args[0], out int x) & int.TryParse(args[1], out int z);
					if(b) {
						options.mcaOffsetX = x;
						options.mcaOffsetZ = z;
						Program.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
					} else {
						Program.WriteWarning("Failed to parse to int");
					}
				} else {
					Program.WriteWarning("Two integers are required!");
				}
			} else if(cmd == "mcasplatmapper") {
				options.useSplatmaps = !options.useSplatmaps;
				Program.WriteLine("MCA splatmapping " + (options.useSplatmaps ? "enabled" : "disabled"));
			}
		}

		public override void AddFormatsToList(List<FileFormat> list) {
			list.Add(new FileFormat("MCA", "mca", "mca", "Minecraft Region File (1.16)", this));
		}

		public override bool Export(string importPath, FileFormat ff, ASCData data, string filename, string fileSubName, ExportOptions exportOptions, Bounds bounds) {
			if(ff.IsFormat("MCA")) {
				int regionX = 0;
				int regionZ = 0;
				if(!string.IsNullOrEmpty(fileSubName)) {
					string[] s = fileSubName.Split('.');
					if(s.Length >= 3) {
						regionX = int.Parse(s[1]);
						regionZ = int.Parse(s[2]);
					}
				}
				return WriteFileMCA(importPath, exportOptions.useSplatmaps, data, filename, exportOptions.subsampling, bounds);
			}
			return false;
		}

		public override string GetSuffixWithExtension(FileFormat fileFormat) {
			return "";
		}

		public override void ValidateExportOptions(ExportOptions options, FileFormat format, ref bool valid, ASCData data) {
			if(options.ContainsFormat("MCA")) {
				bool sourceIs512 = (data.nrows == 512 && data.ncols == 512) || (options.exportRange.NumCols == 512 && options.exportRange.NumRows == 512);
				if(options.fileSplitDims != 512 && !sourceIs512) {
					Program.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
					valid = false;
				}
			}
		}

		public static bool WriteFileMCA(string importPath, bool useSplatmaps, ASCData source, string filename, int subsampling, Bounds bounds) {
			if(subsampling < 1) subsampling = 1;
			float[,] grid = new float[bounds.NumCols / subsampling, bounds.NumRows / subsampling];
			for(int x = 0; x < grid.GetLength(0); x++) {
				for(int y = 0; y < grid.GetLength(1); y++) {
					grid[x, y] = source.data[bounds.xMin + x * subsampling, bounds.yMin + y * subsampling];
				}
			}
			try {
				if(!filename.EndsWith(".mca")) filename += ".mca";
				IExporter exporter = new MinecraftRegionExporter(importPath, grid, true, useSplatmaps);
				ExportUtility.WriteFile(exporter, filename, ExportUtility.GetFormatFromIdenfifier("MCA"));
				return true;
			} catch(Exception e) {
				Program.WriteError("Failed to create Minecraft region file!");
				Program.WriteLine(e.ToString());
				return false;
			}
		}
	}
}
