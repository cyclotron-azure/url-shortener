
#!/bin/bash

# chmod +x clean.sh

find . -name 'obj' -type d -exec rm -rv {} + ; find . -name 'bin' -type d -exec rm -rv {} + ;
