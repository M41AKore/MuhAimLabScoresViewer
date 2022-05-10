# MuhAimLabScoresViewer
A WPF app to pull and present user data from the Aim Lab API


see https://github.com/M41AKore/MuhAimLabScoresViewer/releases for downloads

# Important Download Info!
As my anti-virus program (Defender) registered releases of this app as viruses, I have to assume yours will react the same.
And, of course, the zip does not contain any malicious code.
It'll be necessary for you to temporarily disable the real time protection to download and extract the files.
I'm sorry for this inconvenience but I don't think I can do anything against this at this time.


# Setup
After downloading, run "MuhAimLabScoresViewer.exe" to start the program.
Please go to the settings tab (the gear icon button on the top right) and enter the path 
to your Steam library which houses the workshop downloads for Aim Lab.
Default this should be "C:\Program Files (x86)\Steam\", but you may have made a different library 
on another drive such as "D:\SteamLibrary".

At this point you'd be able to use the function of the "Task" tab to display leaderboards of scenarios 
you've got in your workshop downloads.

Next, you can use the "klutchId finder" to get your unique UserID from the Aim Lab API.
This is used to find you in scoreboards.
Enter the name of a scenario/task (case sensitive) that you have in your workshop downloads and 
you know you've got a score registered, and enter the name you'd have on that leaderboard.
If it's able to find you on that leaderboard, your klutchID will be presented in a textfield to the right of the "find" button.
You can now copy and paste that ID into the "klutchId:" settings field.

At this point you should be able to drag and drop either benchmark or competition files inside of the corresponding tabs to see your scores.
see https://github.com/M41AKore/BenchAndCompFiles/releases for downloads of such files I made so far.

# Disclaimer
I'm a noob programmer and so yes, the code is messy and bad, and yes, there will be bugs.
