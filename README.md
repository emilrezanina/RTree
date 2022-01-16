# R-Tree Visualisation Demo
R-Trees is tree data structures used for spatial access methods (i.e. for indexing multi-dimensional information such as geographical coordinates). The R-Tree was proposed bz Antonin Guttman in 1984 and has found significant use in both theoretical and applied context.

The key idea of the data structure is to group nearby objects and represent them with their minimum bounding rectangle in the next higher level of the tree; the "R" in R-tree is for rectangle. Since all objects lie within this bounding rectangle, a query that does not intersect the bounding rectangle also cannot intersect any of the contained objects. At the leaf level, each rectangle describes a single object; at higher levels the aggregation of an increasing number of objects. This can also be seen as an increasingly coarse approximation of the data set.

<i>From wikipedia - for more information please follow <a href="http://en.wikipedia.org/wiki/R-tree">http://en.wikipedia.org/wiki/R-tree</a>.</i>

This implementation of R-Tree structure is aimed to insert, delete and range search methods. (It doesn't implement build method.)

### This Project provides the follwoing functionality:
<ul>
  <li>Create concrete geometric points by mouse or generate it.</li>
  <li>Visualization of R-Tree structure (current and previous state) with MBR.</li>
  <li>Save actual canvas to .png file.</li>
  <li>Perform range search.</li>
</ul>

### Screenshoots:
![visualization RTree](https://github.com/emilrezanina/RTree/blob/master/screenshots/visualizationRTree.PNG)
![range search RTree](https://github.com/emilrezanina/RTree/blob/master/screenshots/rangeSearchRTree.PNG)

