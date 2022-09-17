# Pikkulin Lemmikki DLC samples

Two of the scripts I wrote for Pikkulin Lemmikki DLC while working for Pikkuli Group Oy as a trainee.

## AudioClipFeedScript

#### Purpose and features

The idea behind this was to tie specific game events to audio narration and other sounds.

This script essentially allows setting up a series of audio clips, in this case a narration, character voices and various action sound effects, and events that are timed to be invoked along with them.

The key data structure is an object that contains:
- an audio clip asset reference
- delay and end padding in seconds
- volume modifier
- event (UnityEvent)

The script plays a clip, possibly waiting before doing so, and then calls methods tied to the event. The end padding time can be negative, resulting in the event being called before before the clip is over and also moving to next AudioClipFeedElement element in the list.

#### In hindsight

Having a UnityEvent in every element is not necessary.

It *might* be better to have a class that contains other fields and a derived class that also has the UnityEvent field. The script would then however have to check the type of each element as it goes through the list. I think it would still be more efficient, but I'm not sure it matters all that much.

The same effect could also be done with two (or more) lists, one for events and one (or more) for audio. Times could be aligned by using a spreadsheet, This would probably be the most efficient way to set up narration, sounds and events, though it would be more difficult to adjust the whole arrangement.

## SlideTransitionManager

#### Purpose and features

This script handles transitions between 2 image slides with image elements to the sides of the main images in the middle. The effect is that of moving a view / camera either vertically or horizontally to another image that is separated from the previous one with filler images on left and right.

The script actually reuses the same four filler images, 2 on both sides, and slides them in and out of view. This means that:
- the view port does not need to move
- main sequential images don't have to be arranged in anny particular way for the next image to appear from any of the four directions

#### In hindsight

The idea is not that complicated, but writing this script was somewhat tedious at the time, which is why I haven't really touched it since I got it to work. There are still some unnecessary actions in the script that are not needed anymore.

Hopefully it's not too annoying to sort out in case someone need to change something in it in the future.