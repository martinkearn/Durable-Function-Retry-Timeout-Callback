﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp.Models
{
    public class MainOrchestrationInput
    {
        /// <summary>
        /// The SendEventPostUri from the management information payload.
        /// </summary>
        public Uri SendEventPostUri { get; set; }
    }
}
