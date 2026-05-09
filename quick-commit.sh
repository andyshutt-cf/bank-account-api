#!/bin/bash

BRANCH=$(git rev-parse --abbrev-ref HEAD)

echo "Current branch: $BRANCH"
echo ""
echo "Files to be staged:"
git status --short
echo ""

read -p "Do you want to stage all changes? (y/n) " -n 1 -r
echo

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted - no changes staged."
    exit 1
fi

git add -A

echo ""
echo "Staged changes:"
git status --short
echo ""

read -p "Enter commit message: " COMMIT_MSG

if [ -z "$COMMIT_MSG" ]; then
    echo "Error: Commit message cannot be empty."
    git reset
    exit 1
fi

git commit -m "$COMMIT_MSG"

if [ $? -ne 0 ]; then
    echo "Commit failed!"
    exit 1
fi

echo ""
read -p "Push to '$BRANCH'? (y/n) " -n 1 -r
echo

if [[ $REPLY =~ ^[Yy]$ ]]; then
    git push origin $BRANCH
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "Successfully pushed to $BRANCH!"
    else
        echo ""
        echo "Push failed! You may need to pull first or check remote access."
        exit 1
    fi
else
    echo "Changes committed locally but not pushed."
fi
