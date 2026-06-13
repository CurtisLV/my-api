#!/bin/bash

# Exit immediately if any command fails
set -e

# Ensure both arguments are provided
if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Error: Missing arguments."
  echo "Usage: $0 <github_username/repo> <docker_username/repo>"
  exit 1
fi

GITHUB_REPO=$1
DOCKER_REPO=$2
TEMP_DIR="temp_clone_dir"

# Cleanup function to run on exit (success or failure)
cleanup() {
  if [ -d "$TEMP_DIR" ]; then
    echo "Cleaning up temporary directory..."
    rm -rf "$TEMP_DIR"
  fi
}
trap cleanup EXIT

# 1. Clone the repository
echo "Cloning https://github.com/${GITHUB_REPO}..."
git clone "https://github.com/${GITHUB_REPO}.git" "$TEMP_DIR"

# 2. Build the Docker image
echo "Building Docker image: ${DOCKER_REPO}..."
cd "$TEMP_DIR"
docker build -t "$DOCKER_REPO" .

# 3. Publish to Docker Hub
echo "Pushing image ${DOCKER_REPO} to Docker Hub..."
docker push "$DOCKER_REPO"

echo "Process completed successfully!"