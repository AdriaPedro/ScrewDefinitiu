-- Version: Lua 5.3.5

local ip="192.168.5.100"
local port=8080
local err=0
local socket=0
local zOffset = 8
local pickPos={armOrientation={-1, -1, -1, -1}, coordinate={0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, tool=0, user=2}
local aproPos={armOrientation={-1, -1, -1, -1}, coordinate={0.0, 0.0, 0.0, 0.0, 0.0, 0.0}, tool=0, user=2}
local Printspeed = 50

SpeedFactor(70)

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
  TCPDestroy(socket)
  Sleep(1000)
  goto create_server
end

while true do

  Move(capturePoint,"User=0 Tool=0 CP=1 SYNC=1")

  -- Open Gripper
  DO(3,OFF)
    DO(2,ON)


      -- Ask for candidate
      while true do
        TCPWrite(socket, "FIND", 0)
        err, buf = TCPRead(socket, 0, "string")
        data =buf.buf
        if err ~= 0 then
          print("Failed to read data, re-connecting")
          TCPDestroy(socket)
          goto create_server
          Sleep(1000)
          break
        end
        if  data== "EMPTY" then
          print("NO CANDIDATES")
        else
          local x, y, rz, width, height = data:match("([^;]+);([^;]+);([^;]+);([^;]+);([^;]+)")

          -- Go to aproache position
          print("x: ", x)
          print("y: ", y)
          print("rz: ", rz)
          print ( "width: ", width)
          print ( "height: ", height)

          aproPos={ coordinate={x, y, zOffset-15, 0.0, 0.0, rz}, tool=0, user=2}
          local status=CheckMove (aproPos)

          if status ~=  0 then
            -- Send move command
            print("Error pose: ", status)
            TCPWrite(socket, "MOVE", 0)
            -- Wait the end of the movement
            err, buf = TCPRead(socket, 0, "string")

          else            
            Move(aproPos)            
            -- Go to pick position
            pickPos={ coordinate={x, y, zOffset, 0.0, 0.0, rz}, tool=0, user=2}
            Move(pickPos, "SYNC=1")



            --Close Gripper
            DO(2,OFF)
              DO(3,ON)
                Sleep(500)

              end
            end

            -- Screw Detection
            Move (P9)
            if 5.5 <= width and width < 6.5 then
              Move(P3)
              if 9 < length and length < 10.5 then
                DOExecute(1, OFF)
                Move (P2)

              elseif 11 < length and length < 12.5 then
                MoveR({-56.88,0,0},"User = 1, Tool = 0")
                DOExecute(1, OFF)
                Move ( P2)

              elseif 13 <= length and length < 14.5 then
                MoveR({-113.76,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 14.5 <= length and length < 15.5 then
                MoveR({-170.64,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 17.5 <= length and length < 18.5 then
                MoveR({-227.52,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 19.5 <= length and length < 20.5 then
                MoveR({-284.4,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)
              else
                Move (P8)
                DOExecute (1, OFF)
                Move ( P2)

              end

            elseif 7.5 <= width and width < 8.5 then
              Move(P3)
              if 9 < length and length < 10.5 then
                DOExecute (1, OFF)
                Move (P2)

              elseif 11 < length and length < 12.5 then
                MoveR({-56.88,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 15 <= length and length < 16.5 then
                MoveR({-113.76,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 19 <= length and length < 20.5 then
                MoveR({-170.64,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 24 <= length and length < 25.5 then
                MoveR({-227.52,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 34 <= length and length < 35.5 then
                MoveR({-284.4,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)
              else
                Move (P8)
                DOExecute (1, OFF)
                Move ( P2)

              end

            elseif 9.5 <= width and width < 10.5 then
              Move(P3)
              if 9 < length and length < 10.5 then
                DOExecute (1, OFF)
                Move (P2)

              elseif 11 < length and length < 12.5 then
                MoveR({-56.88,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 15 <= length and length < 16.5 then
                MoveR({-113.76,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 19 <= length and length < 20.5 then
                MoveR({-170.64,0,0},"User = 1, Tool = 0")
                DOExecute (1, OFF)
                Move ( P2)

              elseif 24 <= length and length < 25.5 then
                MoveR({-227.52,0,0},"User = 1, Tool = 0")
                DO (1, OFF)
                  Move ( P2)

                elseif 34 <= length and length < 35.5 then
                  MoveR({-284.4,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)
                else
                  Move (P8)
                  DOExecute (1, OFF)
                  Move ( P2)

                end

              elseif 11.5 <= width and width < 12.5 then
                Move(P3)
                if 9 < length and length < 10.5 then
                  DOExecute (1, OFF)
                  Move (P2)

                elseif 11 < length and length < 12.5 then
                  MoveR({-56.88,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)

                elseif 15 <= length and length < 16.5 then
                  MoveR({-113.76,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)

                elseif 19 <= length and length < 20.5 then
                  MoveR({-170.64,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)

                elseif 24 <= length and length < 25.5 then
                  MoveR({-227.52,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)

                elseif 34 <= length and length < 35.5 then
                  MoveR({-284.4,0,0},"User = 1, Tool = 0")
                  DOExecute (1, OFF)
                  Move ( P2)
                else
                  Move (P8)
                  DOExecute (1, OFF)
                  Move ( P2)

                end

              else
                Move (P8)
                DOExecute (1, OFF)
                Move ( P2)
              end 

            end
          end