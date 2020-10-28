// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace sample.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}