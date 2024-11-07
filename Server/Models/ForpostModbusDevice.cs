// Backend/Models/ModbusDevice.cs
namespace ForpostModbusTcpPoller.Models
{
    public class ForpostModbusDevice
    {
        public int Id { get; set; } 
        public string IpAddress { get; set; }
        public int Port { get; set; } = 502; // Стандартный порт Modbus TCP
        public byte UnitId => (byte)(Id % 256);
        public ushort RegisterAddress { get; set; } = 60;
        public string RegisterName { get; set; } // Для удобства идентификации
    }
}