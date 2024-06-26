<#@ template hostspecific="true" #>
<#@ output extension=".js" encoding="utf-8" #>
<#@ parameter type="System.String" name="ClassName" #>
const { CreateOpenAI } = require("./CreateOpenAI");
const { OpenAIEnvInfo } = require("./OpenAIEnvInfo");
const { <#= ClassName #> } = require("./OpenAIAssistantsClass");

const readline = require('node:readline/promises');
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

async function main() {

  // Which thread
  const threadId = process.argv[2] || null;

  // Create the OpenAI client
  const openai = await CreateOpenAI.forAssistantsAPI({
    errorCallback: text => process.stdout.write(text)
  });

  // Create the assistants streaming helper class instance
  const assistant = new <#= ClassName #>(OpenAIEnvInfo.ASSISTANT_ID, openai);

  // Get or create the thread, and display the messages if any
  if (threadId === null) {
    await assistant.createThread()
  } else {
    await assistant.retrieveThread(threadId);
    await assistant.getThreadMessages((role, content) => {
      role = role.charAt(0).toUpperCase() + role.slice(1);
      process.stdout.write(`${role}: ${content}`);
      });
  }

  // Loop until the user types 'exit'
  while (true) {

    // Get user input
    const input = await rl.question('User: ');
    if (input === 'exit' || input === '') break;

    // Get the Assistant's response
    let response = await assistant.getResponse(input);
    process.stdout.write(`\nAssistant: ${response}\n\n`);
  }

  console.log(`Bye! (threadId: ${assistant.thread.id})`);
  process.exit();
}

main().catch((err) => {
  if (err.code !== 'ERR_USE_AFTER_CLOSE') { // filter out expected error (EOF on redirected input)
    console.error("The sample encountered an error:", err);
    process.exit(1);
  }
});

module.exports = { main };
