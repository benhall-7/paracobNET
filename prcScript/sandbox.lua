time = os.time
clock = os.clock
date = os.date

if sandbox then
    collectgarbage = nil
    debug = nil
    dofile = nil
    io = nil
    luanet = nil
    load = nil
    loadfile = nil
    os = nil
    package = nil
    require = nil
end