local ip="192.168.5.1"
local port=8080
local err=0
local socket=0
local zOffset = 21
--local pickPos={armOrientation={-1, -1, -1, -1}, coordinate={0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, tool=0, user=2}
local aproPos={armOrientation={-1, -1, -1, -1}, coordinate={0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, tool=0, user=2}
--local Printspeed = 50

-- Values--
local metric1 = 6
local metric2 = 8
local metric3 = 10
local metric4 = 5

local length1 = 10
local length2 = 12
local length3 = 16
local length4 = 20
local length5 = 14
local length6 = 45


SpeedFactor(50)

-- Connect to server.
::create_server::
err, socket = TCPCreate(false, ip, port)
if err ~= 0 then
  print("Failed to create socket, re-connecting")
  Sleep(1000)
  goto create_server
end
err = TCPStart(socket, 0)
if err ~= 0 then
  print("Failed to connect server, re-connecting")
  print(err)
  TCPDestroy(socket)
  Sleep(1000)
  goto create_server
end
while(true) do
  local dataTCP
DO(1, OFF)
Move(P2)
Sync()
TCPWrite(socket, "FIND", 0)
err, dataTCP = TCPRead(socket,0,"string")
print(dataTCP)
--local input = "76.2155;66.2577;-85.2919;7.2858;20.2635"
local input = dataTCP.buf
local x, y, rz, W, H = input:match("([^;]+);([^;]+);([^;]+);([^;]+);([^;]+)")
Sleep(500)
print("x:", x)
print("y:", y)
print("rz:", rz)
print("W:", W)
print("H:", H)
--Move()

-- Save coordinates in P1
aproPos={ coordinate={x, y-9.5, -30, 0.0, 0.0, rz}, tool=0, user=2}
local status=CheckMove (aproPos)


-- Save the size of the screw
width = tonumber(W)
length = tonumber(H)

Move(aproPos)
Sync()
MoveR({0, 0, zOffset},'User=1 Tool=0')
Sync()
DO(1, ON)

Move (P9)
  if (metric1 - 1) <= width and width < (metric1 + 1) then
    Move(P3)
    Sync()
    if (length1 - 1) <= length and length < (length1 + 1) then
      DO(1, OFF)
      Move (P2)

    elseif (length2 - 1) <= length and length < (length2 + 1) then
      MoveR({-56.88,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length3 - 1) <= length and length < (length3 + 1) then
      MoveR({-113.76,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length4 - 1) <= length and length < (length4 + 1) then
      MoveR({-170.64,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length5 - 1) <= length and length < (length5 + 1) then
      MoveR({-227.52,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length6 - 1) <= length and length < (length6 + 1) then
      MoveR({-284.4,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)
    
    else
      Move (P8)
      DO (1, OFF)
      Move ( P2)

    end

  elseif (metric2 - 1) <= width and width < (metric2 + 1) then
    Move(P4)
    Sync()
    if (length1 - 1) <= length and length < (length1 + 1) then
      DO(1, OFF)
      Move (P2)

    elseif (length2 - 1) <= length and length < (length2 + 1) then
      MoveR({-56.88,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length3 - 1) <= length and length < (length3 + 1) then
      MoveR({-113.76,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length4 - 1) <= length and length < (length4 + 1) then
      MoveR({-170.64,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length5 - 1) <= length and length < (length5 + 1) then
      MoveR({-227.52,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length6 - 1) <= length and length < (length6 + 1) then
      MoveR({-284.4,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)
    else
      Move (P8)
      DO (1, OFF)
      Move ( P2)

    end
  elseif (metric3 - 1) <= width and width < (metric3 + 1) then
    Move(P5)
    Sync()
    if (length1 - 1) <= length and length < (length1 + 1) then
      DO(1, OFF)
      Move (P2)

   elseif (length2 - 1) <= length and length < (length2 + 1) then
      MoveR({-56.88,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length3 - 1) <= length and length < (length3 + 1) then
      MoveR({-113.76,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length4 - 1) <= length and length < (length4 + 1) then
      MoveR({-170.64,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length5 - 1) <= length and length < (length5 + 1) then
      MoveR({-227.52,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length6 - 1) <= length and length < (length6 + 1) then
      MoveR({-284.4,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)
    else
      Move (P8)
      DO (1, OFF)
      Move ( P2)

    end 
  elseif (metric4 - 1) <= width and width < (metric4 + 1) then
    Move(P6)
    Sync()
    if (length1 - 1) <= length and length < (length1 + 1) then
      DO(1, OFF)
      Move (P2)

    elseif (length2 - 1) <= length and length < (length2 + 1) then
      MoveR({-56.88,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length3 - 1) <= length and length < (length3 + 1) then
      MoveR({-113.76,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length4 - 1) <= length and length < (length4 + 1) then
      MoveR({-170.64,0,0},"User = 1, Tool = 0")
      DO(1, OFF)
      Move ( P2)

    elseif (length5 - 1) <= length and length < (length5 + 1) then
      MoveR({-227.52,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)

    elseif (length6 - 1) <= length and length < (length6 + 1) then
      MoveR({-284.4,0,0},"User = 1, Tool = 0")
      DO (1, OFF)
      Move ( P2)
    else
      Move (P8)
      DO (1, OFF)
      Move ( P2)

    end            

  else
    Move (P8)
    DO (1, OFF)
    Move ( P2)
  end 
end
