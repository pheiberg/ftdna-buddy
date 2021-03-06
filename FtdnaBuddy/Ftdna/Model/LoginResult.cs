﻿namespace FtdnaBuddy.Ftdna.Model
{
    public class LoginResult
    {
        public string ReturnUrl { get; internal set; }

        public bool IsLockedOut { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public int FailedAccessAttempts { get; internal set; }

        public FtdnaUser User { get; internal set; }
    }
}