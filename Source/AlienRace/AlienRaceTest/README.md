# Alien Race Unit Tests

This subproject adds unit tests to the AlienRaces Mod using the [NUnit](https://docs.nunit.org/) C# testing framework.
Nothing from this project should ever be copied to the Release folder.

## Do I have to use the tests
NO!

We definitely encourage use of the tests to make sure your changes work well but automated testing can be daunting at first.
If this is too much, don't worry about it. It is a great skill to pick up though.
Start slow and ask questions if you need to. Unit tests do not replace play testing.
They are simply another option.

## Why tests
Unit tests allow us to confirm simple things, like that we can parse Genders from XML. As well as that the complex multi-layered fallbacks work as intended given known inputs.

Writing a test for the current behaviour before you make a change allows you to be confident your change hasn't broken what worked before.
It also allows us to test small things, ideally single functions behave as we expect without the complexity of the rest of the Rimworld process.

It also allows us to ask what if style questions like: what does happen if you get a pawn with no backstory?

It also makes it easier to contribute. Contributors can feel happier making changes because they can see there are tests that will catch their mistakes.
They also can copy and adapt existing tests to help them make changes and see what happens on a small scale long before it's all hooked into the game.

It can also help contributors to understand what is actually going on. What *should* happen if there's no graphic for a specific body-addon?
Contributors can read the code but in complex code-bases like this it can be daunting.
Indeed we don't even know if a piece of current behaviour is intended. By writing a test you are asserting what you expect to happen and making sure it won't change unintentionally.
Even the names of tests can answer the questions without needing to actually read the test. Test names should tell you what the expectation is for what input and then prove it!

## How do I run the tests
In Visual Studio or Rider you simply need to right click on the test project and choose `Run unit tests`
The exact wording may vary. This will run the tests and give you a lovely screen telling you how many tests passed and failed
as well as any output and you can double click on tests to go to the code.

If you are using VSCode or Notepad++ or other text editors there are other plugins.
But in general outside of IDEs NUnit provides a console runner and a GUI.
You can read more about this on the [NUnit website](https://docs.nunit.org/articles/nunit/running-tests/Index.html)

To make sure NUnit ConsoleRunner is installed you may need to run `dotnet restore` and or `nuget restore` to install the packages listed in the `packages.config`
This puts the NUnit ConsoleRunner into the `packages` folder.

In order for the tests to find the test data files this command assumes you change directory into the `AlienRaceTest` folder then run a command something like this:

```powershell
..\packages\NUnit.ConsoleRunner.3.15.0\tools\nunit3-console.exe --work=testOutput .\AlienRaceTest.csproj
```

As you can see we call `nunit3-console.exe` we then tell it to dump all the files it may make into the `testOutput` folder using the `--work` argument.
This has been added to the `.gitignore` so we recommend you just use this folder for simplicity.
NUnit makes some log files on every call and because at the CLI you don't have a pretty UI it puts the results in a `TestResult.xml` file.

There are many other options such as `--noresult` and `--inprocess` which can further reduce the output if all you want to see is if the tests pass.

When it's finished you should see something like this:
```text
Test Run Summary
  Overall result: Passed
  Test Count: 16, Passed: 16, Failed: 0, Warnings: 0, Inconclusive: 0, Skipped: 0
  Start time: 2022-07-30 02:55:50Z
    End time: 2022-07-30 02:55:52Z
    Duration: 1.922 seconds
```

As you can see I have 16 passing tests, lovely :)

### Why not just run the tests on Github
That would be ideal. Github can easily run these for you.
However to do so it would need the Rimworld DLLs.
I do not believe the license permits including the Rimworld DLLs themselves.
So the tests must sadly be run on a machine with Rimworld installed.
And thus cannot be run automatically on Github.

## Writing tests
This section gives a little guidance on writing your own tests.

### What should we be testing
Some things are very hard to test particularly in games like Rimworld which make heavy use of reflection and static methods.
Testing harmony patches work as intended for example is really quite hard. These things are not worth spending time on.

You want to test small functions in isolation, using as little of Rimworld as possible.
Rimworld code can change so you mostly want to test only your own code assuming that Rimworld will give you the input you expect.
Should this change you have a clear view of what has changed and can verify things work again after fixing.
You can still test that your harmony patches do as expected in isolation if you like but testing the whole Harmony patching system is probably not worthwhile.

A guiding principle is not to test your libraries so don't bother checking that the library works as intended.

Test any situation you think is worth making clear what you expect to happen.
Also if you find a bug it can be worth writing a test!
Make a test that causes the bug, then fix the bug and boom green test!
This means you fixed it, it won't come back and it can help others not fall into the same trap.

When you have things that read XML, make a test to confirm it can read things as you expect.
Maybe add tests for weird things like what if the backstory has spaces?
I like to test both a tiny piece of config and that a whole block of config can still be read properly and completely.

### Wrapping things that are hard to test
The Rimworld Pawn class is a great example.
It is possible to make a `Verse.Pawn` very easily, but depending on what you do it may try and do things you don't want.
So rather than passing a `Pawn` around directly you can simply wrap it in something you have complete control over.
The `BodyAddonPawnWrapper` has the dual benefit of making the code using pawns a lot easier to read but also,
because that's what is being passed in we can mock that and hide away all the calls to functions that might cause us problems.
Testing the wrapper itself is hard but as long as it is fairly simple we can still test all the hard code.
We don't need to test that Rimworld gives us the right backstory.

### What is a mock?
When you're trying to test your code you want to keep it as isolated as possible.
A mock simply allows you to replace almost any non static object with something that looks the same,
but where it doesn't actually _do_ anything.

If your code needs to call lets say `SomePawn.CurrentBed()`
You don't want to have to go to all the trouble of making a fake Pawn that happens to give the answer you want.
What you care about is what your code does with the returned `Building_Bed`.
So you Mock the pawn making something that looks like a Pawn but always returns a specific bed.
You can chain this on so if a `Building_Bed` is hard to work with,
Simply return a Mock bed as well. Your code will be able to work with it like it was real.
By default though everything will return null or some default.
To make the mock do what you want you have to tell it using the `Setup` methods.

For example:
```csharp
Mock<BodyAddonPawnWrapper> mockPawnWrapper = new Mock<BodyAddonPawnWrapper>();
    mockPawnWrapper.Setup(p => p.CurrentLifeStageDefMatches(this.mockOtherAdultLifestageDef.Object))
                   .Returns(true);
```
First I make a mock that can stand in for a real `BodyAddonPawnWrapper`
Then I say when someone calls `CurrentLifeStageDefMatches`,
specifically when they call it with another mock I made earlier `mockOtherAdultLifestageDef`
then I want it to return true. So in this way I'm saying when you see this thing I gave you earlier, assume it does match that life stage.
But I never had to call the potentially complicated code to do that check. It doesn't matter.
It's just been replaced.

You might notice the `.Object` on that match, that's just because the Mock is of type `Mock<BodyAddonPawnWrapper>`.
The `.Object` simply makes it back into the `BodyAddonPawnWrapper` type that the function expects.

The cool thing is you can then ask the mock after you're done with it what happened to it.
If you were expecting something to happen you can check that it did. Such is the power of Mocking.

Do take a look at some of the existing tests to see how this works.

We are using the [Moq](https://github.com/Moq/moq4/wiki/Quickstart) framework for this.

Mocking is quite verbose though, it's usually not worth Mocking simple things.
Mock complex things that try to interact with other parts of Rimworld and cause you problems.

### Things that we've worked around
Static functions can't easily be replaced or intercepted.
One big example is the logger, this uses some internal unity thing which is hard to swap out in tests.
The TestSupport folder contains a helpful base class which resolves some of these common issues.
Things like the logger and trying to load a save for example.
We use reflection to simply set the field to something we control to avoid calling the code that is hard to work with.

Hopefully you don't run into any of these, if you do it may not be worth your time to fix it.
Unit testing is only one tool in making stable software. If it's too problematic, test a different way and don't worry about it.
