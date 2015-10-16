using System;

namespace vortex.web
{
	public interface ITopicType
	{
		/// <summary>
		/// This method needs to be provided by classes that implement the topic interface
		/// and should be providing the upper and lower 64-bit of the keyhash.
		/// </summary>
		/// <returns>Two long representing the top and bottom 64-bit of the keyHash.</returns>
		Tuple<long, long> keyHash();
	}

	public class KeyHashGenerator {

		public static Tuple<long, long> keyHash(Int16 k) {
			// TODO: Properly take into account endianess requirements of the key-hash.
			return new Tuple<long, long> (0, k);
		}
		public static Tuple<long, long> keyHash(Int32 k) {
			// TODO: Properly take into account endianess requirements of the key-hash.
			return new Tuple<long, long> (0, k);
		}

		public static Tuple<long, long> keyHash (Int64 k) {
			// TODO: Properly take into account endianess requirements of the key-hash.
			return new Tuple<long, long> (0, k);
		}
	}
}

