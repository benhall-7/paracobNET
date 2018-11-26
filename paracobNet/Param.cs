using System;
using System.IO;
/*
check magic,
hashTableSize,
otherTableSize
go to 0x10 + hashTableSize + otherTableSize
label:Start
read command (switch)
if the cmd is not a Group, it must be contained by another Group
	add the command to the containing group and increment the object counter.
	If no parent group exists it will throw an exception (needs root node)
if the cmd is a Group, record the count
	go to offset
	for (int i = 0; i < count; i++) {
		get hash from first index (asynchronous read)
		go to second offset (in order to move the read pointer and enter the loop again, record current address for later)
		begin label:Start once more
		move pointer back to correct address
	}
*/
namespace paracobNet
{
    public class Param
    {
        
    }
}
