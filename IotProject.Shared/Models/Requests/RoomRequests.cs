using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotProject.Shared.Models.Requests
{
	public class RoomCreateRequest
	{
		[Required(ErrorMessage = "Name is required.")]
		public string Name { get; set; }
		public string? Description { get; set; }
	}

	public class RoomUpdateRequest
	{
		public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
		public string? Description { get; set; }
	}
}