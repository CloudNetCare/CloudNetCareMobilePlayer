namespace CloudNetCare.MobilePlayer
{
    class StepResult
    {
        public int StepId { get; set; }

        public string Command { get; set; }
        public string Target { get; set; }
        public string Value { get; set; }

        public string Message { get; set; }
        public string ResultValue { get; set; }
    }
}
