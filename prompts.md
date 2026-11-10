(this file includes prompts and messages i write to AI agent during the chat)

Hello
I have this project which i started several months ago,
the main reason i created this was to have a small, lightweight 
standalone 3d engin written in dotnet that i can define images and 
graphics in 3d in it and i get the 2d projection of it for a specific
 camera angle.
then i had to leave it at very start and now i dont remember what i actually did in some cases.
but here is the code. 
and what i need is to be able to use it easily for creating simple games.
whithout having to deal with the headache of using complicated engins.
i started a simple application with avolonia, and it grow to be a little 
more game-ish, then there was a part of the application that i needed to represent 
a 3d view with prespective. so if it would be also able to integrate 
with avalonia, this would solve my problem.

so what is your suggestion for straight forward road map to compelete 
this 3d-engin project? (i called it Cubism) 



Wow. THis is great. Thanks.
i can see that a cube only defines the geometry and when we create a Scene object cube (calling SceneObject.Cube(.))
it creates a Cube object, then creates face objects based on Cube.Faces with the given color.
i was wondering what if i wanted faces to be filled with texetures? brushes or even images?
how can we update the Face class to support that and regardingly, add another SceneObject.Cube(.) method that can create
this texetured objects

